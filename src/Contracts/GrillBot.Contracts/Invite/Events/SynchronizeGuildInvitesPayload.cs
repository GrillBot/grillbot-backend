namespace GrillBot.Contracts.Invite.Events;

public sealed record SynchronizeGuildInvitesPayload(string GuildId, bool IgnoreLog);
