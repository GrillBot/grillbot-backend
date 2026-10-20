using GrillBot.Contracts.UserManagement.Events;
using Wolverine;

namespace GrillBot.App.Actions.Commands.Infractions;

public class CreateWarning(IMessageBus rabbitPublisher) : CommandAction
{
    public Task ProcessAsync(IGuildUser user, string message, bool notification)
    {
        var payload = new MemberWarningPayload(DateTime.UtcNow, message, user.GuildId, Context.User.Id, user.Id, notification);
        return rabbitPublisher.PublishAsync(payload).AsTask();
    }
}
