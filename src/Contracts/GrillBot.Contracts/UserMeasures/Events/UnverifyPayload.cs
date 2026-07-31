namespace GrillBot.Contracts.UserMeasures.Events;

public sealed record UnverifyPayload(
    DateTime CreatedAtUtc,
    string Reason,
    string GuildId,
    string ModeratorId,
    string TargetUserId,
    DateTime EndAtUtc,
    long LogSetId
) : BasePayload(CreatedAtUtc, Reason, GuildId, ModeratorId, TargetUserId);
