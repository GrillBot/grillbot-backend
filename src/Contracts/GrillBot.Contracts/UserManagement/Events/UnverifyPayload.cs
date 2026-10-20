namespace GrillBot.Contracts.UserManagement.Events;

public sealed record UnverifyPayload(
    DateTime CreatedAtUtc,
    string Reason,
    ulong GuildId,
    ulong ModeratorId,
    ulong TargetUserId,
    DateTime EndAtUtc,
    long LogSetId
) : BasePayload(CreatedAtUtc, Reason, GuildId, ModeratorId, TargetUserId);
