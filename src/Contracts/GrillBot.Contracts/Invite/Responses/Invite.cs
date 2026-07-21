namespace GrillBot.Contracts.Invite.Responses;

public record Invite(
    string Code,
    string GuildId,
    string? CreatorId,
    DateTime? CreatedAt,
    int Uses
);
