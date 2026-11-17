using System.Runtime.CompilerServices;
using AuditLogService.Core.Entity;
using AuditLogService.Core.Extensions;
using GrillBot.Contracts.AuditLog.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuditLogService.Handlers.Archivation;

public partial class StartArchivationHandler
{
    /// <summary>
    /// Yields the records of <paramref name="ids"/> in the given order. Reads one page at a time by primary key,
    /// loads the per-type data for that page and drops it before reading the next one.
    /// </summary>
    private async IAsyncEnumerable<LogItem> ReadItemsToArchiveAsync(
        List<Guid> ids,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var pageNumber = 0;
        var pagesCount = (ids.Count + Options.ReadPageSize - 1) / Options.ReadPageSize;

        foreach (var pageIds in ids.Chunk(Options.ReadPageSize))
        {
            var page = await ReadPageAsync(pageIds, cancellationToken);

            foreach (var typeGroup in page.GroupBy(o => o.Type))
                await FillItemsAsync(typeGroup, typeGroup.Key, cancellationToken);

            foreach (var item in page)
                yield return item;

            pageNumber++;
            if (Logger.IsEnabled(LogLevel.Debug))
                Logger.LogDebug("Audit log archivation read page {Page} of {Pages}.", pageNumber, pagesCount);
        }
    }

    private Task<List<LogItem>> ReadPageAsync(Guid[] pageIds, CancellationToken cancellationToken)
    {
        var query = DbContext.LogItems.AsNoTracking()
            .Include(o => o.Files)
            .Where(o => pageIds.Contains(o.Id))
            .OrderBy(o => o.CreatedAt)
            .ThenBy(o => o.Id);

        return ContextHelper.ReadEntitiesAsync(query, cancellationToken);
    }

    private async Task FillItemsAsync(IEnumerable<LogItem> headers, LogType type, CancellationToken cancellationToken)
    {
        switch (type)
        {
            case LogType.Info or LogType.Warning or LogType.Error:
                await SetLogDataAsync(headers, DbContext.LogMessages, (header, item) => header.LogMessage = item, cancellationToken);
                break;
            case LogType.ChannelCreated:
                await SetLogDataAsync(headers, DbContext.ChannelCreatedItems.Include(o => o.ChannelInfo), (header, item) => header.ChannelCreated = item, cancellationToken);
                break;
            case LogType.ChannelDeleted:
                await SetLogDataAsync(headers, DbContext.ChannelDeletedItems.Include(o => o.ChannelInfo), (header, item) => header.ChannelDeleted = item, cancellationToken);
                break;
            case LogType.ChannelUpdated:
                await SetLogDataAsync(headers, DbContext.ChannelUpdatedItems.Include(o => o.Before).Include(o => o.After), (header, item) => header.ChannelUpdated = item, cancellationToken);
                break;
            case LogType.EmoteDeleted:
                await SetLogDataAsync(headers, DbContext.DeletedEmotes, (header, item) => header.DeletedEmote = item, cancellationToken);
                break;
            case LogType.OverwriteCreated:
                await SetLogDataAsync(headers, DbContext.OverwriteCreatedItems.Include(o => o.OverwriteInfo), (header, item) => header.OverwriteCreated = item, cancellationToken);
                break;
            case LogType.OverwriteDeleted:
                await SetLogDataAsync(headers, DbContext.OverwriteDeletedItems.Include(o => o.OverwriteInfo), (header, item) => header.OverwriteDeleted = item, cancellationToken);
                break;
            case LogType.OverwriteUpdated:
                await SetLogDataAsync(headers, DbContext.OverwriteUpdatedItems.Include(o => o.Before).Include(o => o.After), (header, item) => header.OverwriteUpdated = item, cancellationToken);
                break;
            case LogType.Unban:
                await SetLogDataAsync(headers, DbContext.Unbans, (header, item) => header.Unban = item, cancellationToken);
                break;
            case LogType.MemberUpdated:
                await SetLogDataAsync(headers, DbContext.MemberUpdatedItems.Include(o => o.Before).Include(o => o.After), (header, item) => header.MemberUpdated = item, cancellationToken);
                break;
            case LogType.MemberRoleUpdated:
                await headers.SetLogDataWithoutKeyAsync(DbContext.MemberRoleUpdatedItems, (header, item) =>
                {
                    header.MemberRolesUpdated ??= new HashSet<MemberRoleUpdated>();
                    header.MemberRolesUpdated.Add(item);
                }, ContextHelper, true, cancellationToken);
                break;
            case LogType.GuildUpdated:
                await SetLogDataAsync(headers, DbContext.GuildUpdatedItems.Include(o => o.Before).Include(o => o.After), (header, item) => header.GuildUpdated = item, cancellationToken);
                break;
            case LogType.UserLeft:
                await SetLogDataAsync(headers, DbContext.UserLeftItems, (header, item) => header.UserLeft = item, cancellationToken);
                break;
            case LogType.UserJoined:
                await SetLogDataAsync(headers, DbContext.UserJoinedItems, (header, item) => header.UserJoined = item, cancellationToken);
                break;
            case LogType.MessageEdited:
                await SetLogDataAsync(headers, DbContext.MessageEditedItems, (header, item) => header.MessageEdited = item, cancellationToken);
                break;
            case LogType.MessageDeleted:
                await SetLogDataAsync(headers, DbContext.MessageDeletedItems.Include(o => o.Embeds).ThenInclude(o => o.Fields), (header, item) => header.MessageDeleted = item, cancellationToken);
                break;
            case LogType.InteractionCommand:
                await SetLogDataAsync(headers, DbContext.InteractionCommands, (header, item) => header.InteractionCommand = item, cancellationToken);
                break;
            case LogType.ThreadDeleted:
                await SetLogDataAsync(headers, DbContext.ThreadDeletedItems.Include(o => o.ThreadInfo), (header, item) => header.ThreadDeleted = item, cancellationToken);
                break;
            case LogType.ThreadUpdated:
                await SetLogDataAsync(headers, DbContext.ThreadUpdatedItems.Include(o => o.Before).Include(o => o.After), (header, item) => header.ThreadUpdated = item, cancellationToken);
                break;
            case LogType.JobCompleted:
                await SetLogDataAsync(headers, DbContext.JobExecutions, (header, item) => header.Job = item, cancellationToken);
                break;
            case LogType.Api:
                await SetLogDataAsync(headers, DbContext.ApiRequests, (header, item) => header.ApiRequest = item, cancellationToken);
                break;
            case LogType.RoleDeleted:
                await SetLogDataAsync(headers, DbContext.RoleDeleted.Include(o => o.RoleInfo), (header, item) => header.RoleDeleted = item, cancellationToken);
                break;
            case LogType.Identity:
                await SetLogDataAsync(headers, DbContext.IdentityEvents, (header, item) => header.IdentityEvent = item, cancellationToken);
                break;
            case LogType.Archivation:
                await SetLogDataAsync(headers, DbContext.ArchivationLogs, (header, item) => header.Archivation = item, cancellationToken);
                break;
        }
    }

    private Task SetLogDataAsync<TData>(IEnumerable<LogItem> headers, IQueryable<TData> query, Action<LogItem, TData> setData, CancellationToken cancellationToken)
        where TData : ChildEntityBase
        => headers.SetLogDataAsync(query, setData, ContextHelper, true, cancellationToken);
}
