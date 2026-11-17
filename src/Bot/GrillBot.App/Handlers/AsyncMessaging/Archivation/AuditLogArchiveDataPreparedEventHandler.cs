using System.IO.Compression;
using System.Text.Json.Nodes;
using GrillBot.App.Helpers;
using GrillBot.Common.FileStorage;
using GrillBot.Contracts.AuditLog.Events;
using GrillBot.Contracts.AuditLog.Events.Create;
using GrillBot.Core.IO;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace GrillBot.App.Handlers.AsyncMessaging.Archivation;

/// <summary>
/// Finishes an audit log archivation: enriches the data, zips it with the attachments, stores it,
/// asks AuditLogService to delete the records and writes the Archivation audit log record.
/// Retry-safe: the archive name is derived from the event, so a retry replaces the first upload.
/// </summary>
public class AuditLogArchiveDataPreparedEventHandler(
    GrillBotDatabaseBuilder _databaseBuilder,
    BlobManagerFactoryHelper _blobManagerFactory,
    IMessageBus _messageBus,
    ILogger<AuditLogArchiveDataPreparedEventHandler> _logger
)
{
    public async Task HandleAsync(AuditLogArchiveDataPreparedEvent message, CancellationToken cancellationToken)
    {
        if (message.ItemsCount == 0 || message.Ids.Count == 0)
            return;

        var json = (await CompressedJson.ParseAsync(message.CompressedContent, cancellationToken))?.AsObject()
            ?? throw new InvalidOperationException("Audit log archive data are empty.");

        using (var repository = _databaseBuilder.CreateRepository())
            await ArchiveEnricher.EnrichAsync(repository, json, message.GuildIds, message.ChannelIds, message.UserIds);

        var missingFiles = 0;
        var archiveManager = await _blobManagerFactory.CreateAsync(BlobConstants.AuditLogArchives);
        var result = await ArchiveStorage.StoreAsync(
            archiveManager,
            $"AuditLog_{message.CreatedAtUtc:yyyyMMdd_HHmmss}",
            json,
            async (zip, ct) => missingFiles = await AddFilesAsync(zip, message.Files, ct),
            cancellationToken
        );

        await _messageBus.PublishAsync(new DeleteAuditLogRecordsCommand(message.Ids));

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

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Audit log archive {ArchiveName} stored. Records: {Count}, JSON: {JsonSize} B, ZIP: {ArchiveSize} B, attachments: {Files} ({MissingFiles} missing).",
                result.ArchiveName, message.ItemsCount, result.JsonSize, result.ArchiveSize, message.Files.Count, missingFiles
           );
        }
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
