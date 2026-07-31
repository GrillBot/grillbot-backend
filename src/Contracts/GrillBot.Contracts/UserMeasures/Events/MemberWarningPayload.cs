namespace GrillBot.Contracts.UserMeasures.Events;

public sealed record MemberWarningPayload(
    DateTime CreatedAtUtc,
    string Reason,
    string GuildId,
    string ModeratorId,
    string TargetUserId,
    bool SendDmNotification
) : BasePayload(CreatedAtUtc, Reason, GuildId, ModeratorId, TargetUserId);
