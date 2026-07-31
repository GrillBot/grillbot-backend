using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Core.Redis.Extensions;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using InviteService.Core.Entity;
using InviteService.Models.Cache;
using GrillBot.Contracts.Invite.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace InviteService.Handlers;

public class InviteCreatedEventHandler(
    IServiceProvider serviceProvider,
    IDistributedCache _cache
) : EventHandlerBaseWithDb<InviteContext>(serviceProvider)
{
    public async Task HandleAsync(InviteCreatedPayload message, CancellationToken cancellationToken)
    {
        var key = $"InviteMetadata-{message.GuildId}-{message.Code}";
        var metadata = new InviteMetadata(message.Code, message.Uses, message.CreatorId, message.CreatedAt);

        var invite = await _cache.GetAsync(key, cancellationToken);
        if (invite is not null)
            await _cache.RemoveAsync(key, cancellationToken);
        await _cache.SetAsync(key, metadata, null, cancellationToken);
    }
}
