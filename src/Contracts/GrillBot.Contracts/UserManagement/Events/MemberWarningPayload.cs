namespace GrillBot.Contracts.UserManagement.Events;

public sealed record MemberWarningPayload(
    DateTime CreatedAtUtc,
    string Reason,
    ulong GuildId,
    ulong ModeratorId,
    ulong TargetUserId,
    bool SendDmNotification
) : BasePayload(CreatedAtUtc, Reason, GuildId, ModeratorId, TargetUserId);
