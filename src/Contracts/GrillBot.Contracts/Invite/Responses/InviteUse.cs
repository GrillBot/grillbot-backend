namespace GrillBot.Contracts.Invite.Responses;

public record InviteUse(
    string UserId,
    DateTime UsedAtUtc
);
