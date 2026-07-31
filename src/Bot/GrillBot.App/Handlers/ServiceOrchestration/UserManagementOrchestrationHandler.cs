using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Contracts.UserManagement.Events;
using Wolverine;


namespace GrillBot.App.Handlers.ServiceOrchestration;

public class UserManagementOrchestrationHandler(
    IMessageBus _rabbitPublisher
) : IGuildMemberUpdatedEvent
{
    // GuildMemberUpdated
    public Task ProcessAsync(IGuildUser? before, IGuildUser after)
    {
        return before is null || before.Nickname == after.Nickname
            ? Task.CompletedTask
            : _rabbitPublisher.PublishAsync(NicknameChangedMessage.Create(before, after)).AsTask();
    }
}
