using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Contracts.Unverify.Events;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UnverifyService.Core.Archivation;
using UnverifyService.Core.Entity;
using UnverifyService.Core.Entity.Logs;
using UnverifyService.Options;

namespace UnverifyService.Handlers;

public class LogBulkDeleteHandler(
    IServiceProvider serviceProvider,
    UnverifyLogRetention _retention,
    IOptions<AppOptions> _options
) : EventHandlerBaseWithDb<UnverifyContext>(serviceProvider)
{
    public async Task HandleAsync(DeleteUnverifyLogRecordsCommand message, CancellationToken cancellationToken)
    {
        if (message.Ids.Count == 0)
            return;

        // Nothing deleted also ends an archivation chain: the next run would select the same records again.
        var logItems = await ReadLogItemsAsync(message.Ids, cancellationToken);
        if (logItems.Count == 0)
            return;

        DbContext.RemoveRange(logItems);
        await ContextHelper.SaveChangesAsync(cancellationToken);
        await Publisher.PublishAsync(new RecalculateMetricsMessage());

        if (message.ContinueArchivationSequence is { } sequence)
            await ContinueArchivationAsync(sequence, logItems.Count, cancellationToken);
    }

    /// <summary>
    /// Starts the next archivation run while expired records remain. Published from here, into this service's
    /// own queue, so the run starts only after this delete. Stops after <see cref="ArchivationOptions.MaxChainedRuns"/> runs.
    /// </summary>
    private async Task ContinueArchivationAsync(int sequence, int deleted, CancellationToken cancellationToken)
    {
        var options = _options.Value.Archivation;
        var next = sequence + 1;
        var remaining = await ContextHelper.ReadCountAsync(_retention.WhereExpired(DbContext.LogItems.AsNoTracking()), cancellationToken);

        if (remaining == 0 || remaining < options.MinimalCount)
        {
            if (Logger.IsEnabled(LogLevel.Information))
                Logger.LogInformation("Unverify log archivation finished after {Runs} runs. Deleted in the last run: {Deleted}, expired records left: {Remaining}.", next, deleted, remaining);
            return;
        }

        if (next >= options.MaxChainedRuns)
        {
            Logger.LogWarning("Unverify log archivation stopped after {Runs} chained runs; {Remaining} expired records wait for the next scheduled run.", next, remaining);
            return;
        }

        await Publisher.PublishAsync(new StartUnverifyLogArchivationCommand { Sequence = next });
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
