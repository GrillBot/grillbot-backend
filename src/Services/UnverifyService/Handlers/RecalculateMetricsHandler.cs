using GrillBot.Contracts.Unverify.Events;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using Microsoft.EntityFrameworkCore;
using UnverifyService.Core.Archivation;
using UnverifyService.Core.Entity;
using GrillBot.Contracts.Unverify.Enums;
using UnverifyService.Telemetry;

namespace UnverifyService.Handlers;

public class RecalculateMetricsHandler(
    IServiceProvider serviceProvider,
    UnverifyTelemetryCollector _collector,
    UnverifyLogRetention _retention
) : EventHandlerBaseWithDb<UnverifyContext>(serviceProvider)
{
    public async Task HandleAsync(RecalculateMetricsMessage _, CancellationToken cancellationToken)
    {
        await RecalculateActiveUnverifiesAsync(cancellationToken);
        await RecalculateUnverifyLogsAsync(cancellationToken);
    }

    private async Task RecalculateActiveUnverifiesAsync(CancellationToken cancellationToken = default)
    {
        var query = DbContext.ActiveUnverifies.AsNoTracking()
            .GroupBy(o => o.LogItem.OperationType)
            .Select(o => new { o.Key, Count = o.Count() });

        var data = await ContextHelper.ReadToDictionaryAsync(query, o => o.Key, o => o.Count, cancellationToken);

        _collector.ActiveUnverify.Set(data.TryGetValue(UnverifyOperationType.Unverify, out var unverifyCount) ? unverifyCount : 0);
        _collector.ActiveSelfUnverify.Set(data.TryGetValue(UnverifyOperationType.SelfUnverify, out var selfUnverifyCount) ? selfUnverifyCount : 0);
    }

    private async Task RecalculateUnverifyLogsAsync(CancellationToken cancellationToken = default)
    {
        var query = _retention.WhereExpired(DbContext.LogItems.AsNoTracking());
        _collector.ItemsToArchive.Set(await ContextHelper.ReadCountAsync(query, cancellationToken));
    }
}
