using System.Runtime.CompilerServices;
using GrillBot.Contracts.Unverify.Enums;
using Microsoft.EntityFrameworkCore;
using UnverifyService.Core.Entity.Logs;

namespace UnverifyService.Handlers.Archivation;

public partial class StartLogArchivationHandler
{
    /// <summary>
    /// Yields expired records oldest first, at most <c>MaxItemsPerRun</c>. Reads one page at a time
    /// (keyset on CreatedAt), loads the operations for that page and drops it before reading the next one.
    /// </summary>
    private async IAsyncEnumerable<UnverifyLogItem> ReadItemsToArchiveAsync(
        IQueryable<UnverifyLogItem> expiredQuery,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var remaining = Options.MaxItemsPerRun;
        DateTime? lastCreatedAt = null;
        var lastIds = new List<Guid>();

        while (remaining > 0)
        {
            var pageSize = Math.Min(Options.ReadPageSize, remaining);
            var page = await ReadPageAsync(expiredQuery, lastCreatedAt, lastIds, pageSize, cancellationToken);
            if (page.Count == 0)
                yield break;

            foreach (var group in page.GroupBy(o => o.OperationType))
                await FillItemsAsync(group, group.Key, cancellationToken);

            foreach (var item in page)
                yield return item;

            remaining -= page.Count;
            if (page.Count < pageSize)
                yield break;

            // Records sharing the boundary timestamp are excluded by id on the next page.
            var boundary = page[^1].CreatedAt;
            if (boundary != lastCreatedAt)
                lastIds.Clear();

            lastCreatedAt = boundary;
            lastIds.AddRange(page.Where(o => o.CreatedAt == boundary).Select(o => o.Id));
        }
    }

    private async Task<List<UnverifyLogItem>> ReadPageAsync(
        IQueryable<UnverifyLogItem> expiredQuery,
        DateTime? lastCreatedAt,
        List<Guid> lastIds,
        int pageSize,
        CancellationToken cancellationToken
    )
    {
        var query = expiredQuery.AsNoTracking();

        if (lastCreatedAt is not null)
        {
            var createdAt = lastCreatedAt.Value;
            var excludedIds = lastIds.ToList();

            query = query.Where(o => o.CreatedAt > createdAt || (o.CreatedAt == createdAt && !excludedIds.Contains(o.Id)));
        }

        query = query
            .OrderBy(o => o.CreatedAt)
            .ThenBy(o => o.Id)
            .Take(pageSize);

        return await ContextHelper.ReadEntitiesAsync(query, cancellationToken);
    }

    private async Task FillItemsAsync(IEnumerable<UnverifyLogItem> items, UnverifyOperationType type, CancellationToken cancellationToken)
    {
        var itemsById = items.ToDictionary(o => o.Id);

        switch (type)
        {
            case UnverifyOperationType.Unverify or UnverifyOperationType.SelfUnverify:
                {
                    var query = CreateFillQuery<UnverifyLogSetOperation>(itemsById.Keys, q => q.Include(o => o.Channels).Include(o => o.Roles));
                    foreach (var operation in await ContextHelper.ReadEntitiesAsync(query, cancellationToken))
                        itemsById[operation.LogItemId].SetOperation = operation;
                }
                break;
            case UnverifyOperationType.AutoRemove or UnverifyOperationType.ManualRemove or UnverifyOperationType.Recovery:
                {
                    var query = CreateFillQuery<UnverifyLogRemoveOperation>(itemsById.Keys, q => q.Include(o => o.Roles).Include(o => o.Channels));
                    foreach (var operation in await ContextHelper.ReadEntitiesAsync(query, cancellationToken))
                        itemsById[operation.LogItemId].RemoveOperation = operation;
                }
                break;
            case UnverifyOperationType.Update:
                {
                    var query = CreateFillQuery<UnverifyLogUpdateOperation>(itemsById.Keys);
                    foreach (var operation in await ContextHelper.ReadEntitiesAsync(query, cancellationToken))
                        itemsById[operation.LogItemId].UpdateOperation = operation;
                }
                break;
        }
    }

    private IQueryable<TEntity> CreateFillQuery<TEntity>(
        IEnumerable<Guid> logIds,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null
    ) where TEntity : UnverifyOperationBase
    {
        var ids = logIds.ToArray();

        var query = DbContext.Set<TEntity>()
            .AsNoTracking()
            .Where(o => ids.Contains(o.LogItemId));

        return includes is not null ? includes(query).AsSplitQuery() : query;
    }
}
