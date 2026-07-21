using Discord;

namespace GrillBot.Models.Events.Messages.Components;

public class ButtonComponent
{
    public string? Label { get; set; }
    public string CustomId { get; set; } = null!;
    public ButtonStyle Style { get; set; }
    public string? EmoteId { get; set; }
    public string? Url { get; set; }
    public bool IsDisabled { get; set; }

    public ButtonComponent()
    {
    }

    public ButtonComponent(string? label, string customId, ButtonStyle style, string? emoteId = null, string? url = null, bool isDisabled = false)
    {
        Label = label;
        CustomId = customId;
        Style = style;
        EmoteId = emoteId;
        Url = url;
        IsDisabled = isDisabled;
    }

    public ButtonBuilder ToBuilder()
    {
        var emote = CreateEmote();

        return new ButtonBuilder()
            .WithLabel(Label)
            .WithCustomId(CustomId)
            .WithStyle(Style)
            .WithUrl(Url)
            .WithEmote(emote)
            .WithDisabled(IsDisabled);
    }

    private IEmote? CreateEmote()
    {
        if (string.IsNullOrEmpty(EmoteId))
            return null;

        if (Discord.Emote.TryParse(EmoteId, out var emote))
            return emote;
        if (Emoji.TryParse(EmoteId, out var emoji))
            return emoji;

        return null;
    }

    public Discord.ButtonComponent ToDiscordComponent()
        => ToBuilder().Build();

    public static ButtonComponent FromDiscordComponent(Discord.ButtonComponent component)
    {
        return new ButtonComponent(
            component.Label,
            component.CustomId,
            component.Style,
            component.Emote?.ToString(),
            component.Url,
            component.IsDisabled
        );
    }
}
