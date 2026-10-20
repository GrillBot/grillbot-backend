namespace GrillBot.Contracts.UserManagement.Events;

public sealed record TimeoutPayload(
    DateTime CreatedAtUtc,
    string Reason,
    ulong GuildId,
    ulong ModeratorId,
    ulong TargetUserId,
    DateTime ValidToUtc,
    long ExternalId
) : BasePayload(CreatedAtUtc, Reason, GuildId, ModeratorId, TargetUserId);
