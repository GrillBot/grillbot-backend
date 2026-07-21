namespace GrillBot.Contracts.Invite.Responses;

public record UserInviteUse(
    string GuildId,
    string Code,
    DateTime JoinedAtUtc
);
