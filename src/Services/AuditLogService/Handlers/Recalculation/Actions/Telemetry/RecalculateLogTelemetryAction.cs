using AuditLogService.Core.Archivation;
using GrillBot.Contracts.AuditLog.Events.Recalculation;
using AuditLogService.Telemetry;
using GrillBot.Services.Common.EntityFramework.Extensions;

namespace AuditLogService.Handlers.Recalculation.Actions.Telemetry;

public class RecalculateLogTelemetryAction(IServiceProvider serviceProvider) : RecalculationActionBase(serviceProvider)
{
    private readonly AuditLogTelemetryCollector _telemetryCollector
        = serviceProvider.GetRequiredService<AuditLogTelemetryCollector>();

    private readonly AuditLogRetention _retention
        = serviceProvider.GetRequiredService<AuditLogRetention>();

    public override async Task ProcessAsync(RecalculationPayload payload)
    {
        await RecalculateItemsToArchiveAsync();
    }

    private async Task RecalculateItemsToArchiveAsync()
    {
        var query = _retention.WhereExpired(DbContext.LogItems);
        var count = await query.CountAsync();

        _telemetryCollector.ItemsOfArchive.Set(count);
    }
}
