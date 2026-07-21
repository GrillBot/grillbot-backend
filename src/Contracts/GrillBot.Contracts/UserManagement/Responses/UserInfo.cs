namespace GrillBot.Contracts.UserManagement.Responses;

public record UserInfo(
    string UserId,
    List<GuildUser> Guilds,
    TimeSpan? SelfUnverifyMinimalTime
);
