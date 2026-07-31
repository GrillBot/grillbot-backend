using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Contracts.Unverify.Events;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using UnverifyService.Core.Entity;

namespace UnverifyService.Handlers;

public class GuildUserLeftHandler(
    IServiceProvider serviceProvider
) : EventHandlerBaseWithDb<UnverifyContext>(serviceProvider)
{
    public async Task HandleAsync(GuildUserLeftMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var activeUnverifyQuery = DbContext.ActiveUnverifies.Where(o => o.LogItem.GuildId == message.GuildId && o.LogItem.ToUserId == message.UserId);
            var activeUnverify = await ContextHelper.ReadFirstOrDefaultEntityAsync(activeUnverifyQuery, cancellationToken);

            if (activeUnverify is null)
                return;

            DbContext.Remove(activeUnverify);
            await ContextHelper.SaveChangesAsync(cancellationToken);
            return;
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
                await Publisher.PublishAsync(new RecalculateMetricsMessage());
        }
    }
}
