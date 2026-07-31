using Discord;
using GrillBot.Contracts.Bot.Events.Messages.Components;
using GrillBot.Contracts.Bot.Events.Messages.Embeds;

namespace GrillBot.Contracts.Bot.Events.Messages;

public sealed record DiscordEditMessagePayload : DiscordMessagePayloadData
{
    public ulong? GuildId { get; init; }
    public ulong ChannelId { get; init; }
    public ulong MessageId { get; init; }

    public DiscordEditMessagePayload(
        ulong? guildId,
        ulong channelId,
        ulong messageId,
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
        MessageId = messageId;
    }

    public static async Task<DiscordEditMessagePayload> FromMessageAsync(
        IMessage message,
        string serviceId,
        Func<IAttachment, Task<byte[]>> downloadAttachmentTask
    )
    {
        var guildId = message.Channel is ITextChannel channel ? channel.GuildId : (ulong?)null;
        var attachments = new List<DiscordMessageFile>();

        foreach (var attachment in message.Attachments)
        {
            attachments.Add(new(
                attachment.Filename,
                attachment.IsSpoiler(),
                await downloadAttachmentTask(attachment).ConfigureAwait(false),
                attachment.Description
            ));
        }

        return new DiscordEditMessagePayload(
            guildId,
            message.Channel.Id,
            message.Id,
            message.Content,
            attachments,
            serviceId,
            null,
            message.Flags,
            message.Embeds.Count > 0 ? DiscordMessageEmbed.FromEmbed(message.Embeds.First()) : null,
            null,
            DiscordMessageComponent.FromComponents(message.Components)
        );
    }
}
