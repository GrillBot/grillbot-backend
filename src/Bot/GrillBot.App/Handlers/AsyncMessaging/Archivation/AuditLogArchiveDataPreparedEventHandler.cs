using System.IO.Compression;
using System.Text.Json.Nodes;
using GrillBot.App.Helpers;
using GrillBot.Common.FileStorage;
using GrillBot.Contracts.AuditLog.Events;
using GrillBot.Contracts.AuditLog.Events.Create;
using GrillBot.Core.IO;
using GrillBot.Core.IO.SharedStorage;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace GrillBot.App.Handlers.AsyncMessaging.Archivation;

/// <summary>
/// Finishes an audit log archivation: reads the records from the shared storage, adds the lookups, zips them with
/// the attachments, stores the ZIP, asks AuditLogService to delete the records (and to continue the chain) and
/// writes the Archivation audit log record. The records are streamed into the ZIP, never parsed.
/// Retry-safe: the archive name is derived from the event, so a retry replaces the first upload; when the shared
/// file is already gone and the archive exists, an earlier attempt finished and only the delete is repeated.
/// </summary>
public class AuditLogArchiveDataPreparedEventHandler(
    GrillBotDatabaseBuilder _databaseBuilder,
    BlobManagerFactoryHelper _blobManagerFactory,
    SharedFileStorage _sharedStorage,
    IMessageBus _messageBus,
    ILogger<AuditLogArchiveDataPreparedEventHandler> _logger
)
{
    public async Task HandleAsync(AuditLogArchiveDataPreparedEvent message, CancellationToken cancellationToken)
    {
        if (message.ItemsCount == 0 || message.Ids.Count == 0)
            return;

        var baseName = $"AuditLog_{message.CreatedAtUtc:yyyyMMdd_HHmmss}";
        var archiveManager = await _blobManagerFactory.CreateAsync(BlobConstants.AuditLogArchives);
        var deleteCommand = new DeleteAuditLogRecordsCommand(message.Ids)
        {
            ContinueArchivationSequence = message.HasMore ? message.Sequence : null
        };

        if (!_sharedStorage.Exists(message.ContentFileName))
        {
            await RepeatDeleteOfStoredArchiveAsync(archiveManager, baseName, message.ContentFileName, deleteCommand);
            return;
        }

        JsonObject lookups;
        using (var repository = _databaseBuilder.CreateRepository())
            lookups = await ArchiveEnricher.CreateLookupsAsync(repository, message.GuildIds, message.ChannelIds, message.UserIds);

        var missingFiles = 0;
        ArchiveStorage.Result result;
        await using (var compressed = _sharedStorage.OpenRead(message.ContentFileName))
        await using (var data = CompressedJson.OpenRead(compressed))
        {
            result = await ArchiveStorage.StoreAsync(
                archiveManager,
                baseName,
                data,
                lookups,
                async (zip, ct) => missingFiles = await AddFilesAsync(zip, message.Files, ct),
                cancellationToken
            );
        }

        await _messageBus.PublishAsync(deleteCommand);

        var notes = missingFiles > 0 ? new[] { $"Attachments not found in storage: {missingFiles}" } : null;
        await _messageBus.PublishAsync(ArchivationReport.CreateLogMessage(new ArchivationRequest
        {
            ProcessName = ArchivationProcessNames.AuditLog,
            TotalRecordsCount = message.ItemsCount,
            JsonSize = result.JsonSize,
            ArchiveSize = result.ArchiveSize,
            Details = ArchivationReport.BuildDetails(result.ArchiveName, message.PerType, message.PerMonths, notes),
            DeletedFilesCount = message.Files.Count,
            DeletedFilesSize = message.TotalFilesSize
        }));

        _sharedStorage.Delete(message.ContentFileName);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Audit log archive {ArchiveName} stored (run {Sequence}, more pending: {HasMore}). Records: {Count}, JSON: {JsonSize} B, ZIP: {ArchiveSize} B, attachments: {Files} ({MissingFiles} missing).",
                result.ArchiveName, message.Sequence, message.HasMore, message.ItemsCount, result.JsonSize, result.ArchiveSize, message.Files.Count, missingFiles
           );
        }
    }

    /// <summary>
    /// The shared file is gone. If the archive is stored, an earlier attempt got past the upload; the delete is
    /// repeated (deleting already deleted records does nothing). Otherwise the data are lost and the event fails.
    /// </summary>
    private async Task RepeatDeleteOfStoredArchiveAsync(BlobManager archiveManager, string baseName, string contentFileName, DeleteAuditLogRecordsCommand deleteCommand)
    {
        var archiveName = ArchiveStorage.GetArchiveName(baseName);
        if (!await archiveManager.ExistsAsync(archiveName))
        {
            throw new InvalidOperationException(
                $"Audit log archive data {contentFileName} are not in the shared storage and archive {archiveName} was never stored. The records stay and are archived by a later run."
            );
        }

        _logger.LogWarning("Audit log archive {ArchiveName} is already stored and its data file {FileName} is gone; repeating only the delete.", archiveName, contentFileName);
        await _messageBus.PublishAsync(deleteCommand);
    }

    /// <summary>Streams the attachments into the archive one by one. Returns how many were not found.</summary>
    private async Task<int> AddFilesAsync(ZipArchive zip, List<string> files, CancellationToken cancellationToken)
    {
        if (files.Count == 0)
            return 0;

        // Current container first, the legacy one as a fallback.
        BlobManager[] sources =
        [
            await _blobManagerFactory.CreateAsync(BlobConstants.AuditLogDeletedAttachments),
            await _blobManagerFactory.CreateLegacyAsync()
        ];

        var missing = 0;

        foreach (var file in files.Distinct())
        {
            var source = await FindSourceAsync(sources, file);
            if (source is null)
            {
                missing++;
                continue;
            }

            var entry = zip.CreateEntry(file);
            entry.LastWriteTime = DateTimeOffset.UtcNow;

            await using var entryStream = await entry.OpenAsync(cancellationToken);
            await source.DownloadToAsync(file, entryStream, cancellationToken);
        }

        return missing;
    }

    /// <summary>The first container that has the file, or null when none has it.</summary>
    private static async Task<BlobManager?> FindSourceAsync(IEnumerable<BlobManager> sources, string file)
    {
        foreach (var source in sources)
        {
            if (await source.ExistsAsync(file))
                return source;
        }

        return null;
    }
}
