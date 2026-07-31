using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using MessageService.Core.Entity;
using GrillBot.Contracts.Message.Events;
using Microsoft.EntityFrameworkCore;

namespace MessageService.Handlers.MessageReceived;

public partial class MessageReceivedHandler(IServiceProvider serviceProvider) : EventHandlerBaseWithDb<MessageContext>(serviceProvider)
{
    public async Task HandleAsync(MessageReceivedPayload message, CancellationToken cancellationToken)
    {
        var channelQuery = DbContext.GuildChannels.AsNoTracking()
            .Where(o => o.GuildId == message.GuildId && o.ChannelId == message.ChannelId && !o.IsDeleted);
        var channel = (await ContextHelper.ReadFirstOrDefaultEntityAsync(channelQuery, cancellationToken)) ?? new();

        if (!channel.IsPointsDisabled)
            await ProcessPointsTransactionRequestAsync(message);

        if (!channel.IsAutoReplyDisabled)
            await ProcessAutoReplyAsync(message);
    }
}
