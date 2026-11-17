using AuditLogService.Core.Archivation;
using AuditLogService.Core.Entity;
using AuditLogService.Core.Options;
using GrillBot.Contracts.AuditLog.Events;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using Microsoft.Extensions.Options;

namespace AuditLogService.Handlers.Archivation;

/// <summary>
/// Builds one archive of expired audit log records and publishes it as <see cref="AuditLogArchiveDataPreparedEvent"/>.
/// Records are streamed page by page into a compressed JSON writer, so memory use is bounded by
/// <see cref="ArchivationOptions.ReadPageSize"/>, not by the size of the archive.
/// </summary>
public partial class StartArchivationHandler(
    IServiceProvider serviceProvider,
    AuditLogRetention _retention,
    IOptions<AppOptions> _options
) : EventHandlerBaseWithDb<AuditLogServiceContext>(serviceProvider)
{
    private ArchivationOptions Options => _options.Value.Archivation;

    public async Task HandleAsync(StartAuditLogArchivationCommand _, CancellationToken cancellationToken)
    {
        var expired = _retention.CreatePolicy().BuildExpiredPredicate(DateTime.UtcNow);

        var count = await ContextHelper.ReadCountAsync(DbContext.LogItems.Where(expired), cancellationToken);
        if (count == 0 || count < Options.MinimalItemsToArchive)
        {
            if (Logger.IsEnabled(LogLevel.Information))
                Logger.LogInformation("Audit log archivation skipped. Expired records: {Count}, minimum: {Minimum}.", count, Options.MinimalItemsToArchive);
            return;
        }

        var archive = await CreateArchiveAsync(ReadItemsToArchiveAsync(expired, cancellationToken), cancellationToken);
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
                "Audit log archive prepared. Records: {Count}, JSON: {JsonSize} B, compressed: {CompressedSize} B.",
                archive.ItemsCount, archive.JsonSize, archive.CompressedContent.LongLength
            );
        }

        await Publisher.PublishAsync(archive);
    }
}
