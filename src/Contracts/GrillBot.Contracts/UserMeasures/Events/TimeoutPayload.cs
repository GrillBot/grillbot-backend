namespace GrillBot.Contracts.UserMeasures.Events;

public sealed record TimeoutPayload(
    DateTime CreatedAtUtc,
    string Reason,
    string GuildId,
    string ModeratorId,
    string TargetUserId,
    DateTime ValidToUtc,
    long ExternalId
) : BasePayload(CreatedAtUtc, Reason, GuildId, ModeratorId, TargetUserId);
