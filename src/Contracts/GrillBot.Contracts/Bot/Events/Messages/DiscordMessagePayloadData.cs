using Discord;
using GrillBot.Contracts.Bot.Events.Messages.Components;
using GrillBot.Contracts.Bot.Events.Messages.Embeds;

namespace GrillBot.Contracts.Bot.Events.Messages;

/// <summary>
/// Everything a Discord message carries, shared by the send and edit events.
/// A single constructor per record keeps the shape unambiguous for the serializer.
/// </summary>
public abstract record DiscordMessagePayloadData
{
    public LocalizedMessageContent? Content { get; init; }
    public List<DiscordMessageFile> Attachments { get; init; } = [];
    public DiscordMessageEmbed? Embed { get; init; }
    public MessageFlags? Flags { get; init; }
    public DiscordMessageAllowedMentions? AllowedMentions { get; init; }
    public string ServiceId { get; init; } = null!;
    public Dictionary<string, string> ServiceData { get; init; } = [];
    public DiscordMessageComponent? Components { get; init; }
    public DiscordMessageReference? Reference { get; init; }

    public bool CanUseLocalization => ServiceData.TryGetValue("UseLocalization", out var _useLocalization) && _useLocalization == "true";
    public string? Locale => ServiceData.TryGetValue("Language", out var _locale) ? _locale : null;

    protected DiscordMessagePayloadData(
        LocalizedMessageContent? content,
        List<DiscordMessageFile> attachments,
        string serviceId,
        DiscordMessageAllowedMentions? allowedMentions = null,
        MessageFlags? flags = null,
        DiscordMessageEmbed? embed = null,
        Dictionary<string, string>? serviceData = null,
        DiscordMessageComponent? components = null,
        DiscordMessageReference? reference = null
    )
    {
        Content = content;
        Attachments = [.. attachments.Where(o => o is not null)];
        Embed = embed;
        Flags = flags;
        AllowedMentions = allowedMentions;
        ServiceId = serviceId;
        ServiceData = serviceData ?? [];
        Components = components;
        Reference = reference;
    }

    public DiscordMessagePayloadData WithLocalization(bool useLocalization = true, string? locale = null)
    {
        if (useLocalization)
            ServiceData.TryAdd("UseLocalization", "true");
        if (!string.IsNullOrEmpty(locale))
            ServiceData.TryAdd("Language", locale);

        return this;
    }
}
