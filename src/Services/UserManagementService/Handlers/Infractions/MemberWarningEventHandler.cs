using Discord;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Contracts.Bot.Events.Messages;
using GrillBot.Contracts.Bot.Events.Messages.Embeds;
using GrillBot.Services.Common.Discord;
using UserManagementService.Core.Entity.Infractions;
using GrillBot.Contracts.UserManagement.Events;

namespace UserManagementService.Handlers.Infractions;

public class MemberWarningEventHandler(
    IServiceProvider serviceProvider,
    DiscordManager _discordManager
) : BaseInfractionHandler(serviceProvider)
{
    public async Task HandleAsync(MemberWarningPayload message, CancellationToken cancellationToken)
    {
        var entity = new WarningInfraction
        {
            CreatedAtUtc = message.CreatedAtUtc.ToUniversalTime(),
            GuildId = message.GuildId,
            ModeratorId = message.ModeratorId,
            Reason = message.Reason,
            UserId = message.TargetUserId
        };

        await SaveEntityAsync(entity);

        if (message.SendDmNotification)
            await SendNotificationToUserAsync(entity);
    }

    private async Task SendNotificationToUserAsync(WarningInfraction item)
    {
        var guild = await _discordManager.GetGuildAsync(item.GuildId);

        var embed = new DiscordMessageEmbed(
            null,
            "Obdržel jsi upozornění",
            null,
            null,
            Color.Red.RawValue,
            null,
            null,
            null,
            [
                new DiscordMessageEmbedField("Server", guild?.Name ?? "Neznámý server", true),
                new DiscordMessageEmbedField("Obsah varování", item.Reason, false)
            ],
            null,
            true
        );

        var message = new DiscordSendMessagePayload(null, item.UserId, null, [], "UserManagement", null, null, embed);
        await Publisher.PublishAsync(message);
    }
}
