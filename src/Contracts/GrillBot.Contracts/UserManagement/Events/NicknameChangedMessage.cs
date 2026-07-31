using Discord;

namespace GrillBot.Contracts.UserManagement.Events;

public sealed record NicknameChangedMessage(ulong GuildId, ulong UserId, string? NicknameBefore, string? NicknameAfter)
{
    public static NicknameChangedMessage Create(IGuildUser userBefore, IGuildUser userAfter)
        => new(userBefore.Guild.Id, userBefore.Id, userBefore.Nickname, userAfter.Nickname);
}
