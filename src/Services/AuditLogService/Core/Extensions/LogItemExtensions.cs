using AuditLogService.Core.Entity;
using GrillBot.Services.Common.EntityFramework.Helpers;
using Microsoft.EntityFrameworkCore;

#pragma warning disable S4144 // Methods should not have identical implementations
namespace AuditLogService.Core.Extensions;

public static class LogItemExtensions
{
    public static async Task SetLogDataAsync<TData>(
        this IEnumerable<LogItem> headers,
        IQueryable<TData> query,
        Action<LogItem, TData> setData,
        ContextHelper<AuditLogServiceContext> contextHelper,
        bool disableTracking,
        CancellationToken cancellationToken = default
    ) where TData : ChildEntityBase
    {
        var headersById = headers.ToDictionary(o => o.Id);
        var ids = headersById.Keys.ToList();

        query = query.Where(o => ids.Contains(o.LogItemId));
        if (disableTracking)
            query = query.AsNoTracking();

        var data = await contextHelper.ReadEntitiesAsync(query, cancellationToken);
        foreach (var item in data)
        {
            setData(headersById[item.LogItemId], item);
        }
    }

    public static async Task SetLogDataWithoutKeyAsync<TData>(
        this IEnumerable<LogItem> headers,
        IQueryable<TData> query,
        Action<LogItem, TData> setData,
        ContextHelper<AuditLogServiceContext> contextHelper,
        bool disableTracking,
        CancellationToken cancellationToken = default
    ) where TData : ChildEntityBaseWithoutKey
    {
        var headersById = headers.ToDictionary(o => o.Id);
        var ids = headersById.Keys.ToList();

        query = query.Where(o => ids.Contains(o.LogItemId));
        if (disableTracking)
            query = query.AsNoTracking();

        var data = await contextHelper.ReadEntitiesAsync(query, cancellationToken);
        foreach (var item in data)
        {
            setData(headersById[item.LogItemId], item);
        }
    }
}
