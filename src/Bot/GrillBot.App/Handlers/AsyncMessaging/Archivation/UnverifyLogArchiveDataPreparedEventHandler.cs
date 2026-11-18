using System.Text.Json.Nodes;
using GrillBot.App.Helpers;
using GrillBot.Common.FileStorage;
using GrillBot.Contracts.AuditLog.Events.Create;
using GrillBot.Contracts.Unverify.Events;
using GrillBot.Core.IO;
using GrillBot.Core.IO.SharedStorage;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace GrillBot.App.Handlers.AsyncMessaging.Archivation;

/// <summary>
/// Finishes an unverify log archivation: reads the records from the shared storage, adds the lookups, zips and
/// stores them, asks UnverifyService to delete the records (and to continue the chain) and writes the Archivation
/// audit log record. The records are streamed into the ZIP, never parsed.
/// Retry-safe: the archive name is derived from the event, so a retry replaces the first upload; when the shared
/// file is already gone and the archive exists, an earlier attempt finished and only the delete is repeated.
/// </summary>
public class UnverifyLogArchiveDataPreparedEventHandler(
    GrillBotDatabaseBuilder _databaseBuilder,
    BlobManagerFactoryHelper _blobManagerFactory,
    SharedFileStorage _sharedStorage,
    IMessageBus _messageBus,
    ILogger<UnverifyLogArchiveDataPreparedEventHandler> _logger
)
{
    public async Task HandleAsync(UnverifyLogArchiveDataPreparedEvent message, CancellationToken cancellationToken)
    {
        if (message.ItemsCount == 0 || message.Ids.Count == 0)
            return;

        var baseName = $"UnverifyLog_{message.CreatedAtUtc:yyyyMMdd_HHmmss}";
        var archiveManager = await _blobManagerFactory.CreateAsync(BlobConstants.UnverifyLogArchives);
        var deleteCommand = new DeleteUnverifyLogRecordsCommand(message.Ids)
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
        {
            lookups = await ArchiveEnricher.CreateLookupsAsync(
                repository,
                message.GuildIds.Select(o => o.ToString()),
                message.ChannelIds.Select(o => o.ToString()),
                message.UserIds.Select(o => o.ToString())
            );
        }

        ArchiveStorage.Result result;
        await using (var compressed = _sharedStorage.OpenRead(message.ContentFileName))
        await using (var data = CompressedJson.OpenRead(compressed))
            result = await ArchiveStorage.StoreAsync(archiveManager, baseName, data, lookups, null, cancellationToken);

        await _messageBus.PublishAsync(deleteCommand);

        await _messageBus.PublishAsync(ArchivationReport.CreateLogMessage(new ArchivationRequest
        {
            ProcessName = ArchivationProcessNames.Unverify,
            TotalRecordsCount = message.ItemsCount,
            JsonSize = result.JsonSize,
            ArchiveSize = result.ArchiveSize,
            Details = ArchivationReport.BuildDetails(
                result.ArchiveName,
                message.PerType.Select(o => KeyValuePair.Create(o.Key, (long)o.Value)),
                message.PerMonths.Select(o => KeyValuePair.Create(o.Key, (long)o.Value))
            )
        }));

        _sharedStorage.Delete(message.ContentFileName);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Unverify log archive {ArchiveName} stored (run {Sequence}, more pending: {HasMore}). Records: {Count}, JSON: {JsonSize} B, ZIP: {ArchiveSize} B.",
                result.ArchiveName, message.Sequence, message.HasMore, message.ItemsCount, result.JsonSize, result.ArchiveSize
            );
        }
    }

    /// <summary>
    /// The shared file is gone. If the archive is stored, an earlier attempt got past the upload; the delete is
    /// repeated (deleting already deleted records does nothing). Otherwise the data are lost and the event fails.
    /// </summary>
    private async Task RepeatDeleteOfStoredArchiveAsync(BlobManager archiveManager, string baseName, string contentFileName, DeleteUnverifyLogRecordsCommand deleteCommand)
    {
        var archiveName = ArchiveStorage.GetArchiveName(baseName);
        if (!await archiveManager.ExistsAsync(archiveName))
        {
            throw new InvalidOperationException(
                $"Unverify log archive data {contentFileName} are not in the shared storage and archive {archiveName} was never stored. The records stay and are archived by a later run."
            );
        }

        _logger.LogWarning("Unverify log archive {ArchiveName} is already stored and its data file {FileName} is gone; repeating only the delete.", archiveName, contentFileName);
        await _messageBus.PublishAsync(deleteCommand);
    }
}
