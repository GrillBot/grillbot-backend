using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Contracts.Unverify.Events;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using Microsoft.EntityFrameworkCore;
using UnverifyService.Core.Entity;
using UnverifyService.Core.Entity.Logs;

namespace UnverifyService.Handlers;

public class LogBulkDeleteHandler(
    IServiceProvider serviceProvider
) : EventHandlerBaseWithDb<UnverifyContext>(serviceProvider)
{
    public async Task HandleAsync(LogBulkDeleteMessage message, CancellationToken cancellationToken)
    {
        if (message.Ids.Count == 0)
            return;

        var logItems = await ReadLogItemsAsync(message.Ids, cancellationToken);
        if (logItems.Count == 0)
            return;

        DbContext.RemoveRange(logItems);
        await ContextHelper.SaveChangesAsync(cancellationToken);
        await Publisher.PublishAsync(new RecalculateMetricsMessage());
        return;
    }

    private async Task<List<UnverifyLogItem>> ReadLogItemsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var result = new List<UnverifyLogItem>();

        var baseQuery = DbContext.LogItems
            .Include(o => o.ChildLogItems)
            .Include(o => o.RemoveOperation!).ThenInclude(o => o.Channels)
            .Include(o => o.RemoveOperation!).ThenInclude(o => o.Roles)
            .Include(o => o.SetOperation!).ThenInclude(o => o.Channels)
            .Include(o => o.SetOperation!).ThenInclude(o => o.Roles)
            .Include(o => o.UpdateOperation)
            .Where(o => o.ActiveUnverify == null)
            .AsSplitQuery();

        foreach (var chunk in ids.Distinct().Chunk(100))
        {
            var query = baseQuery.Where(o => chunk.Contains(o.Id));
            result.AddRange(await ContextHelper.ReadEntitiesAsync(query, cancellationToken));
        }

        return result;
    }
}
