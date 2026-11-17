using GrillBot.App.Helpers;
using GrillBot.Common.FileStorage;
using GrillBot.Contracts.AuditLog.Events.Create;
using GrillBot.Contracts.Unverify.Events;
using GrillBot.Core.IO;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace GrillBot.App.Handlers.AsyncMessaging.Archivation;

/// <summary>
/// Finishes an unverify log archivation: enriches the data, zips and stores it, asks UnverifyService
/// to delete the records and writes the Archivation audit log record.
/// Retry-safe: the archive name is derived from the event, so a retry replaces the first upload.
/// </summary>
public class UnverifyLogArchiveDataPreparedEventHandler(
    GrillBotDatabaseBuilder _databaseBuilder,
    BlobManagerFactoryHelper _blobManagerFactory,
    IMessageBus _messageBus,
    ILogger<UnverifyLogArchiveDataPreparedEventHandler> _logger
)
{
    public async Task HandleAsync(UnverifyLogArchiveDataPreparedEvent message, CancellationToken cancellationToken)
    {
        if (message.ItemsCount == 0 || message.Ids.Count == 0)
            return;

        var json = (await CompressedJson.ParseAsync(message.CompressedContent, cancellationToken))?.AsObject()
            ?? throw new InvalidOperationException("Unverify log archive data are empty.");

        using (var repository = _databaseBuilder.CreateRepository())
        {
            await ArchiveEnricher.EnrichAsync(
                repository,
                json,
                message.GuildIds.Select(o => o.ToString()),
                message.ChannelIds.Select(o => o.ToString()),
                message.UserIds.Select(o => o.ToString())
            );
        }

        var archiveManager = await _blobManagerFactory.CreateAsync(BlobConstants.UnverifyLogArchives);
        var result = await ArchiveStorage.StoreAsync(
            archiveManager,
            $"UnverifyLog_{message.CreatedAtUtc:yyyyMMdd_HHmmss}",
            json,
            null,
            cancellationToken
        );

        await _messageBus.PublishAsync(new DeleteUnverifyLogRecordsCommand(message.Ids));

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

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Unverify log archive {ArchiveName} stored. Records: {Count}, JSON: {JsonSize} B, ZIP: {ArchiveSize} B.",
                result.ArchiveName, message.ItemsCount, result.JsonSize, result.ArchiveSize
            );
        }
    }
}
