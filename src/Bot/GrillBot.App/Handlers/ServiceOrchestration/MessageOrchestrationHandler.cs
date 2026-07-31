using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Contracts.Message.Events;
using GrillBot.Contracts.Message.Events.Channels;
using Wolverine;


namespace GrillBot.App.Handlers.ServiceOrchestration;

public class MessageOrchestrationHandler(
    IMessageBus _publisher
) : IMessageReceivedEvent, IThreadDeletedEvent, IChannelDestroyedEvent
{
    // MessageReceived
    public Task ProcessAsync(IMessage message)
    {
        var payload = MessageReceivedPayload.Create(message);
        return payload is not null ? _publisher.PublishAsync(payload).AsTask() : Task.CompletedTask;
    }

    // ThreadDeleted
    public Task ProcessAsync(IThreadChannel? cachedThread, ulong threadId)
    {
        if (cachedThread is null)
            return Task.CompletedTask;

        var syncItem = ChannelSynchronizationItem.FromChannel(cachedThread) with { IsDeleted = true };

        return _publisher.PublishAsync(new SynchronizationPayload([syncItem])).AsTask();
    }

    // ChannelDestroyed
    public Task ProcessAsync(IChannel channel)
    {
        if (channel is not IGuildChannel guildChannel)
            return Task.CompletedTask;

        var syncItem = ChannelSynchronizationItem.FromChannel(guildChannel) with { IsDeleted = true };

        return _publisher.PublishAsync(new SynchronizationPayload([syncItem])).AsTask();
    }
}
