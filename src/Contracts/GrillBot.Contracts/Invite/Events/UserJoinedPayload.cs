using GrillBot.Core.RabbitMQ.V2.Messages;

namespace GrillBot.Contracts.Invite.Events;

public class UserJoinedPayload : IRabbitMessage
{
    public string Topic => "Invite";
    public string Queue => "UserJoined";

    public string GuildId { get; set; } = null!;
    public string UserId { get; set; } = null!;

    public UserJoinedPayload()
    {
    }

    public UserJoinedPayload(string guildId, string userId)
    {
        GuildId = guildId;
        UserId = userId;
    }
}
