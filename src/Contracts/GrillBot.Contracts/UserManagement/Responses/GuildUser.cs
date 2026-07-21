using GrillBot.Contracts.Unverify.Responses;

namespace GrillBot.Contracts.UserManagement.Responses;

public record GuildUser(
    string GuildId,
    string? CurrentNickname,
    List<string> NicknameHistory,
    UnverifyInfo? CurrentUnverify,
    int UnverifyCount,
    int SelfUnverifyCount,
    int TimeoutCount,
    int WarningCount
);
