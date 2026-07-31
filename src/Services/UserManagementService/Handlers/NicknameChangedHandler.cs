using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Contracts.AuditLog.Events.Create;
using GrillBot.Services.Common.Discord;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using System.Text;
using UserManagementService.Core.Entity;
using GrillBot.Contracts.UserManagement.Events;

namespace UserManagementService.Handlers;

public class NicknameChangedHandler(
    IServiceProvider serviceProvider,
    DiscordManager _discordManager
) : EventHandlerBaseWithDb<UserManagementContext>(serviceProvider)
{
    public async Task HandleAsync(NicknameChangedMessage message, CancellationToken cancellationToken)
    {
        if (message.NicknameBefore == message.NicknameAfter)
            return;

        var user = await _discordManager.GetUserAsync(message.UserId, cancellationToken);
        if (user is null)
            return;

        var isUser = !(user.IsBot || user.IsWebhook);
        var nicknameBefore = SanitizeNickname(message.NicknameBefore, isUser);
        var nicknameAfter = SanitizeNickname(message.NicknameAfter, isUser);

        await UpdateNicknameHistoryAsync(message.GuildId, message.UserId, nicknameBefore);
        await UpdateNicknameHistoryAsync(message.GuildId, message.UserId, nicknameAfter);
        await UpdateCurrentNicknameAsync(message, nicknameAfter);
        await ContextHelper.SaveChangesAsync(cancellationToken);
        await NotifyAuditLogAsync(message);
    }

    private async Task UpdateNicknameHistoryAsync(ulong guildId, ulong userId, string? nickname)
    {
        if (string.IsNullOrEmpty(nickname))
            return;

        var existsQuery = DbContext.Nicknames.Where(o => o.UserId == userId && o.GuildId == guildId && o.Value == nickname);
        if (await ContextHelper.IsAnyAsync(existsQuery))
            return;

        await DbContext.AddAsync(new GuildUserNickname
        {
            GuildId = guildId,
            UserId = userId,
            Value = nickname
        });
    }

    private async Task UpdateCurrentNicknameAsync(NicknameChangedMessage message, string? currentNickname)
    {
        var guildUserQuery = DbContext.GuildUsers.Where(o => o.GuildId == message.GuildId && o.UserId == message.UserId);
        var guildUser = await ContextHelper.ReadFirstOrDefaultEntityAsync(guildUserQuery);

        if (guildUser is null)
        {
            guildUser = new GuildUser
            {
                GuildId = message.GuildId,
                UserId = message.UserId,
            };

            await DbContext.AddAsync(guildUser);
        }

        guildUser.CurrentNickname = currentNickname;
    }

    private Task NotifyAuditLogAsync(NicknameChangedMessage message)
    {
        var logRequest = new LogRequest(LogType.MemberUpdated, DateTime.UtcNow, message.GuildId.ToString())
        {
            MemberUpdated = new MemberUpdatedRequest { UserId = message.UserId.ToString() }
        };

        return Publisher.PublishAsync(new CreateItemsMessage(logRequest)).AsTask();
    }

    private static string? SanitizeNickname(string? nickname, bool isUser)
    {
        if (string.IsNullOrEmpty(nickname))
            return null;

        var builder = new StringBuilder();
        var nicknameValue = isUser ? nickname : nickname.Cut(32, true)!;

        foreach (var character in nicknameValue.Where(ch => Rune.TryCreate(ch, out _)))
            builder.Append(character);

        return builder.ToString();
    }
}
