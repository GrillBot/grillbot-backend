namespace GrillBot.Contracts.UserManagement.Events;

public abstract record BasePayload(
    DateTime CreatedAtUtc,
    string Reason,
    ulong GuildId,
    ulong ModeratorId,
    ulong TargetUserId
);
