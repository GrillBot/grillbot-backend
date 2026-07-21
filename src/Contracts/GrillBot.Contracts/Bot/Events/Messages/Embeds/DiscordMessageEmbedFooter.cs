using Discord;

namespace GrillBot.Contracts.Bot.Events.Messages.Embeds;

public class DiscordMessageEmbedFooter
{
    public LocalizedMessageContent? Text { get; set; }
    public LocalizedMessageContent? IconUrl { get; set; }

    public DiscordMessageEmbedFooter()
    {
    }

    public DiscordMessageEmbedFooter(string? text, string? iconUrl)
    {
        Text = string.IsNullOrEmpty(text) ? null : new(text, []);
        IconUrl = string.IsNullOrEmpty(iconUrl) ? null : new(iconUrl, []);
    }

    public EmbedFooterBuilder ToBuilder()
    {
        var builder = new EmbedFooterBuilder();
        if (!string.IsNullOrEmpty(Text?.Key))
            builder = builder.WithText(Text.Key);
        if (!string.IsNullOrEmpty(IconUrl?.Key))
            builder = builder.WithIconUrl(IconUrl.Key);
        return builder;
    }

    public static DiscordMessageEmbedFooter? FromEmbed(EmbedFooter? footer)
        => footer is null ? null : new(footer.Value.Text, footer.Value.IconUrl);

    public static DiscordMessageEmbedFooter? FromEmbed(EmbedFooterBuilder? builder)
        => builder is null ? null : new(builder.Text, builder.IconUrl);
}
