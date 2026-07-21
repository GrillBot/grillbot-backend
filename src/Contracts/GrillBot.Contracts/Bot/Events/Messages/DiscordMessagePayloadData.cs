using Discord;
using GrillBot.Contracts.Bot.Events.Messages.Components;
using GrillBot.Contracts.Bot.Events.Messages.Embeds;

namespace GrillBot.Contracts.Bot.Events.Messages;

public class DiscordMessagePayloadData
{
    public LocalizedMessageContent? Content { get; set; }
    public List<DiscordMessageFile> Attachments { get; set; } = [];
    public DiscordMessageEmbed? Embed { get; set; }
    public MessageFlags? Flags { get; set; }
    public DiscordMessageAllowedMentions? AllowedMentions { get; set; }
    public string ServiceId { get; set; } = null!;
    public Dictionary<string, string> ServiceData { get; set; } = [];
    public DiscordMessageComponent? Components { get; set; }
    public DiscordMessageReference? Reference { get; set; }

    public bool CanUseLocalization => ServiceData.TryGetValue("UseLocalization", out var _useLocalization) && _useLocalization == "true";
    public string? Locale => ServiceData.TryGetValue("Language", out var _locale) ? _locale : null;

    public DiscordMessagePayloadData()
    {
    }

    public DiscordMessagePayloadData(
        LocalizedMessageContent? content,
        IEnumerable<DiscordMessageFile> attachments,
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
            ServiceData.Add("UseLocalization", "true");
        if (!string.IsNullOrEmpty(locale))
            ServiceData.TryAdd("Language", locale);

        return this;
    }
}
