using Discord;
using GrillBot.Contracts.Bot.Events.Messages.Components;
using GrillBot.Contracts.Bot.Events.Messages.Embeds;

namespace GrillBot.Contracts.Bot.Events.Messages;

public sealed record DiscordSendMessagePayload : DiscordMessagePayloadData
{
    public ulong? GuildId { get; init; }
    public ulong ChannelId { get; init; }

    public DiscordSendMessagePayload(
        ulong? guildId,
        ulong channelId,
        LocalizedMessageContent? content,
        List<DiscordMessageFile> attachments,
        string serviceId,
        DiscordMessageAllowedMentions? allowedMentions = null,
        MessageFlags? flags = null,
        DiscordMessageEmbed? embed = null,
        Dictionary<string, string>? serviceData = null,
        DiscordMessageComponent? components = null,
        DiscordMessageReference? reference = null
    ) : base(content, attachments, serviceId, allowedMentions, flags, embed, serviceData, components, reference)
    {
        GuildId = guildId;
        ChannelId = channelId;
    }
}
