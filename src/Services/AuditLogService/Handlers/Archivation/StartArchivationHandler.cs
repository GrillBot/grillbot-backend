using System.Diagnostics;
using AuditLogService.Core.Archivation;
using AuditLogService.Core.Entity;
using AuditLogService.Core.Options;
using AuditLogService.Telemetry;
using GrillBot.Contracts.AuditLog.Events;
using GrillBot.Core.IO.SharedStorage;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using Microsoft.Extensions.Options;

namespace AuditLogService.Handlers.Archivation;

/// <summary>
/// Builds one archive of expired audit log records into the shared storage and publishes its file name as
/// <see cref="AuditLogArchiveDataPreparedEvent"/>. The retention query runs once to pick the records; they are
/// then read by id, page by page, into a compressed JSON file, so memory use is bounded by
/// <see cref="ArchivationOptions.ReadPageSize"/>, and the message stays small whatever the archive size.
/// </summary>
public partial class StartArchivationHandler(
    IServiceProvider serviceProvider,
    AuditLogRetention _retention,
    AuditLogTelemetryCollector _telemetryCollector,
    SharedFileStorage _sharedStorage,
    IOptions<AppOptions> _options
) : EventHandlerBaseWithDb<AuditLogServiceContext>(serviceProvider)
{
    /// <summary>Prefix of the archive files in the shared storage.</summary>
    public const string SharedFilePrefix = "AuditLog";

    private ArchivationOptions Options => _options.Value.Archivation;

    public async Task HandleAsync(StartAuditLogArchivationCommand command, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        DeleteLeftoverFiles();

        var expired = _retention.QueryExpired(DbContext);

        var count = await ContextHelper.ReadCountAsync(expired, cancellationToken);
        _telemetryCollector.ItemsOfArchive.Set(count);

        if (count == 0 || count < Options.MinimalItemsToArchive)
        {
            if (Logger.IsEnabled(LogLevel.Information))
                Logger.LogInformation("Audit log archivation skipped. Expired records: {Count}, minimum: {Minimum}.", count, Options.MinimalItemsToArchive);
            return;
        }

        var ids = await ReadIdsToArchiveAsync(expired, cancellationToken);
        if (Logger.IsEnabled(LogLevel.Information))
        {
            Logger.LogInformation(
                "Audit log archivation started (run {Sequence}). Expired records: {Count}, archiving {Selected} in this run. Selection took {Elapsed} ms.",
                command.Sequence, count, ids.Count, stopwatch.ElapsedMilliseconds
            );
        }

        // An uncommitted file (failure, or nothing written) is deleted when the writer is disposed.
        await using var file = _sharedStorage.Create(SharedFilePrefix, ".json.br");

        var archive = await CreateArchiveAsync(ReadItemsToArchiveAsync(ids, cancellationToken), file.Stream, cancellationToken);
        if (archive.ItemsCount == 0)
            return;

        await file.CommitAsync(cancellationToken);
        archive = archive with
        {
            ContentFileName = file.Name,
            Sequence = command.Sequence,
            HasMore = count > archive.ItemsCount
        };

        if (Logger.IsEnabled(LogLevel.Information))
        {
            Logger.LogInformation(
                "Audit log archive {FileName} prepared. Records: {Count}, JSON: {JsonSize} B, compressed: {CompressedSize} B, more pending: {HasMore}, took {Elapsed} ms.",
                archive.ContentFileName, archive.ItemsCount, archive.JsonSize, archive.CompressedSize, archive.HasMore, stopwatch.ElapsedMilliseconds
            );
        }

        await Publisher.PublishAsync(archive);
    }

    /// <summary>Files of crashed runs and of events that were never consumed (dead-lettered).</summary>
    private void DeleteLeftoverFiles()
    {
        var deleted = _sharedStorage.DeleteExpired(SharedFilePrefix, Options.SharedFileRetention);
        if (deleted > 0)
            Logger.LogWarning("Deleted {Count} audit log archive files older than {Retention} from the shared storage. Their archives were never stored.", deleted, Options.SharedFileRetention);
    }

    /// <summary>Ids of the oldest expired records, at most <see cref="ArchivationOptions.MaxItemsToArchivePerRun"/>.</summary>
    private Task<List<Guid>> ReadIdsToArchiveAsync(IQueryable<LogItem> expired, CancellationToken cancellationToken)
    {
        var query = expired
            .OrderBy(o => o.CreatedAt)
            .ThenBy(o => o.Id)
            .Select(o => o.Id)
            .Take(Options.MaxItemsToArchivePerRun);

        return ContextHelper.ReadEntitiesAsync(query, cancellationToken);
    }
}
