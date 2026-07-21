using Discord;
using GrillBot.Core.RabbitMQ.V2.Messages;
using GrillBot.Contracts.Bot.Events.Messages.Components;
using GrillBot.Contracts.Bot.Events.Messages.Embeds;

namespace GrillBot.Contracts.Bot.Events.Messages;

public class DiscordSendMessagePayload : DiscordMessagePayloadData, IRabbitMessage
{
    public string Topic => "GrillBot";
    public string Queue => "SendMessage";

    public ulong? GuildId { get; set; }
    public ulong ChannelId { get; set; }

    public DiscordSendMessagePayload()
    {
    }

    public DiscordSendMessagePayload(
        ulong? guildId,
        ulong channelId,
        LocalizedMessageContent? content,
        IEnumerable<DiscordMessageFile> attachments,
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
