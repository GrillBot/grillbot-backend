namespace GrillBot.Contracts.UserMeasures.Events;

public abstract record BasePayload(
    DateTime CreatedAtUtc,
    string Reason,
    string GuildId,
    string ModeratorId,
    string TargetUserId
);
