using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using MessageService.Core.Entity;
using GrillBot.Contracts.Message.Events;
using GrillBot.Contracts.Message.Events.Channels;

namespace MessageService.Handlers;

public class SynchronizationEventHandler(IServiceProvider serviceProvider) : EventHandlerBaseWithDb<MessageContext>(serviceProvider)
{
    public async Task HandleAsync(SynchronizationPayload message, CancellationToken cancellationToken)
    {
        foreach (var channel in message.Channels)
            await SynchronizeChannelAsync(channel);

        await ContextHelper.SaveChangesAsync(cancellationToken);
        return;
    }

    private async Task SynchronizeChannelAsync(ChannelSynchronizationItem channel)
    {
        var channelQuery = DbContext.GuildChannels.Where(o => o.ChannelId == channel.ChannelId && o.GuildId == channel.GuildId);
        var entity = await ContextHelper.ReadFirstOrDefaultEntityAsync(channelQuery);

        if (entity is null)
        {
            entity = new GuildChannel
            {
                ChannelId = channel.ChannelId,
                GuildId = channel.GuildId
            };

            await DbContext.AddAsync(entity);
        }

        entity.IsDeleted = channel.IsDeleted ?? entity.IsDeleted;
        entity.IsPointsDisabled = channel.IsPointsDisabled ?? entity.IsPointsDisabled;
        entity.IsAutoReplyDisabled = channel.IsAutoReplyDisabled ?? entity.IsAutoReplyDisabled;
    }
}
