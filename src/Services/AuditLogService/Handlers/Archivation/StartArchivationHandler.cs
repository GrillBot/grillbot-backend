using System.Diagnostics;
using AuditLogService.Core.Archivation;
using AuditLogService.Core.Entity;
using AuditLogService.Core.Options;
using AuditLogService.Telemetry;
using GrillBot.Contracts.AuditLog.Events;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using Microsoft.Extensions.Options;

namespace AuditLogService.Handlers.Archivation;

/// <summary>
/// Builds one archive of expired audit log records and publishes it as <see cref="AuditLogArchiveDataPreparedEvent"/>.
/// The retention query runs once to pick the records; they are then read by id, page by page, into a
/// compressed JSON writer, so memory use is bounded by <see cref="ArchivationOptions.ReadPageSize"/>.
/// </summary>
public partial class StartArchivationHandler(
    IServiceProvider serviceProvider,
    AuditLogRetention _retention,
    AuditLogTelemetryCollector _telemetryCollector,
    IOptions<AppOptions> _options
) : EventHandlerBaseWithDb<AuditLogServiceContext>(serviceProvider)
{
    private ArchivationOptions Options => _options.Value.Archivation;

    public async Task HandleAsync(StartAuditLogArchivationCommand _, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
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
                "Audit log archivation started. Expired records: {Count}, archiving {Selected} in this run. Selection took {Elapsed} ms.",
                count, ids.Count, stopwatch.ElapsedMilliseconds
            );
        }

        var archive = await CreateArchiveAsync(ReadItemsToArchiveAsync(ids, cancellationToken), cancellationToken);
        if (archive.ItemsCount == 0)
            return;

        if (archive.CompressedContent.LongLength > Options.CompressedSizeWarningThreshold)
        {
            Logger.LogWarning(
                "Audit log archive is {Size} bytes compressed ({Count} records), above the {Threshold} bytes threshold. Lower Archivation:MaxItemsToArchivePerRun if the broker rejects it.",
                archive.CompressedContent.LongLength, archive.ItemsCount, Options.CompressedSizeWarningThreshold
            );
        }

        if (Logger.IsEnabled(LogLevel.Information))
        {
            Logger.LogInformation(
                "Audit log archive prepared. Records: {Count}, JSON: {JsonSize} B, compressed: {CompressedSize} B, took {Elapsed} ms.",
                archive.ItemsCount, archive.JsonSize, archive.CompressedContent.LongLength, stopwatch.ElapsedMilliseconds
            );
        }

        await Publisher.PublishAsync(archive);
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
