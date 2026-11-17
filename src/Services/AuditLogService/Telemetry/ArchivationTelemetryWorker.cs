using AuditLogService.Core.Archivation;
using AuditLogService.Core.Entity;
using AuditLogService.Core.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuditLogService.Telemetry;

/// <summary>
/// Keeps the <c>items_to_archive</c> metric current. The count changes not only when records are created
/// or deleted, but also as time passes and older records reach the end of their retention, so it is
/// recalculated on a timer (<see cref="ArchivationOptions.TelemetryRefreshInterval"/>) instead of per message.
/// The archivation run and the bulk delete refresh it too, right after they change it.
/// </summary>
public class ArchivationTelemetryWorker(
    IServiceScopeFactory _scopeFactory,
    IOptions<AppOptions> _options,
    AuditLogTelemetryCollector _collector,
    ILogger<ArchivationTelemetryWorker> _logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.Value.Archivation.TelemetryRefreshInterval);

        do
        {
            try
            {
                await RefreshAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Recalculation of the count of audit log records to archive failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var retention = scope.ServiceProvider.GetRequiredService<AuditLogRetention>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditLogServiceContext>();
        var retentionQuery = retention.QueryExpired(dbContext);

        _collector.ItemsOfArchive.Set(await retentionQuery.CountAsync(cancellationToken));
    }
}
