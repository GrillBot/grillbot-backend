using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Contracts.Unverify.Events;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using Microsoft.EntityFrameworkCore;
using UnverifyService.Actions.Archivation;
using UnverifyService.Core.Entity;
using GrillBot.Contracts.Unverify.Enums;
using UnverifyService.Telemetry;

namespace UnverifyService.Handlers;

public class RecalculateMetricsHandler(
    IServiceProvider serviceProvider,
    UnverifyTelemetryCollector _collector,
    CreateArchivationDataAction _archivationAction
) : EventHandlerBaseWithDb<UnverifyContext>(serviceProvider)
{
    public async Task HandleAsync(RecalculateMetricsMessage message, CancellationToken cancellationToken)
    {
        await RecalculateActiveUnverifiesAsync(cancellationToken);
        await RecalculateUnverifyLogsAsync(cancellationToken);

        return;
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
        _archivationAction.SetCancellationToken(cancellationToken);

        _collector.ItemsToArchive.Set(await _archivationAction.CountAsync());
    }
}
