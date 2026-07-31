namespace GrillBot.Contracts.Invite.Events;

public sealed record UserJoinedPayload(string GuildId, string UserId);
