using Discord;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Contracts.Bot.Events.Messages;
using GrillBot.Contracts.Bot.Events.Messages.Embeds;
using GrillBot.Services.Common.Discord;
using UserMeasuresService.Core.Entity;
using UserMeasuresService.Handlers.Abstractions;
using GrillBot.Contracts.UserMeasures.Events;

namespace UserMeasuresService.Handlers;

public class MemberWarningEventHandler(
    IServiceProvider serviceProvider,
    DiscordManager _discordManager
) : BaseMeasuresHandler(serviceProvider)
{
    public async Task HandleAsync(MemberWarningPayload message, CancellationToken cancellationToken)
    {
        var entity = new MemberWarningItem
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
        return;
    }

    private async Task SendNotificationToUserAsync(MemberWarningItem item)
    {
        var guild = await _discordManager.GetGuildAsync(item.GuildId.ToUlong());

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

        var message = new DiscordSendMessagePayload(null, item.UserId.ToUlong(), null, [], "UserMeasures", null, null, embed);
        await Publisher.PublishAsync(message);
    }
}
