using Discord;

namespace GrillBot.Models.Events.Messages;

public class DiscordMessageReference
{
    public ulong? MessageId { get; set; }
    public ulong? ChannelId { get; set; }
    public ulong? GuildId { get; set; }
    public bool FailIfNotExists { get; set; } = true;
    public MessageReferenceType ReferenceType { get; set; } = MessageReferenceType.Default;

    public DiscordMessageReference()
    {
    }

    public DiscordMessageReference(ulong? messageId, ulong? channelId = null, ulong? guildId = null, bool failIfNotExists = true,
        MessageReferenceType referenceType = MessageReferenceType.Default)
    {
        MessageId = messageId;
        ChannelId = channelId;
        GuildId = guildId;
        FailIfNotExists = failIfNotExists;
        ReferenceType = referenceType;
    }

    public MessageReference ToDiscordReference()
        => new(MessageId, ChannelId, GuildId, FailIfNotExists, ReferenceType);
}
