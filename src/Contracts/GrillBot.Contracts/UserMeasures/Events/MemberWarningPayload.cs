namespace GrillBot.Contracts.UserMeasures.Events;

public class MemberWarningPayload : BasePayload
{
    public override string Queue => "MemberWarning";

    public bool SendDmNotification { get; set; }

    public MemberWarningPayload()
    {
    }

    public MemberWarningPayload(DateTime createdAt, string reason, string guildId, string moderatorId, string targetUserId, bool sendDmNotification)
        : base(createdAt, reason, guildId, moderatorId, targetUserId)
    {
        SendDmNotification = sendDmNotification;
    }
}
