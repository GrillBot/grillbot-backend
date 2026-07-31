using GrillBot.Contracts.UserMeasures.Events;
using Wolverine;


namespace GrillBot.App.Actions.Commands.UserMeasures;

public class CreateUserMeasuresWarning : CommandAction
{
    private readonly IMessageBus _rabbitPublisher;

    public CreateUserMeasuresWarning(IMessageBus rabbitPublisher)
    {
        _rabbitPublisher = rabbitPublisher;
    }

    public Task ProcessAsync(IGuildUser user, string message, bool notification)
    {
        var moderatorId = Context.User.Id.ToString();
        var guildId = user.GuildId.ToString();

        var payload = new MemberWarningPayload(DateTime.UtcNow, message, guildId, moderatorId, user.Id.ToString(), notification);
        return _rabbitPublisher.PublishAsync(payload).AsTask();
    }
}
