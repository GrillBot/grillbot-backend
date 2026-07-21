using Discord;

namespace GrillBot.Contracts.Bot.Events.Messages.Embeds;

public class DiscordMessageEmbed
{
    public LocalizedMessageContent? Url { get; set; }
    public LocalizedMessageContent? Title { get; set; }
    public LocalizedMessageContent? Description { get; set; }
    public DiscordMessageEmbedAuthor? Author { get; set; }
    public uint? Color { get; set; }
    public DiscordMessageEmbedFooter? Footer { get; set; }
    public LocalizedMessageContent? ImageUrl { get; set; }
    public LocalizedMessageContent? ThumbnailUrl { get; set; }
    public List<DiscordMessageEmbedField> Fields { get; set; } = [];
    public DateTime? Timestamp { get; set; }
    public bool UseCurrentTimestamp { get; set; }

    public DiscordMessageEmbed()
    {
    }

    public DiscordMessageEmbed(
        LocalizedMessageContent? url,
        LocalizedMessageContent? title,
        LocalizedMessageContent? description,
        DiscordMessageEmbedAuthor? author,
        uint? color,
        DiscordMessageEmbedFooter? footer,
        LocalizedMessageContent? imageUrl,
        LocalizedMessageContent? thumbnailUrl,
        IEnumerable<DiscordMessageEmbedField> fields,
        DateTime? timestamp,
        bool useCurrentTimestamp
    )
    {
        Url = url;
        Title = title;
        Description = description;
        Author = author;
        Color = color;
        Footer = footer;
        ImageUrl = imageUrl;
        ThumbnailUrl = thumbnailUrl;
        Fields = [.. fields.Take(EmbedBuilder.MaxFieldCount)];
        Timestamp = timestamp;
        UseCurrentTimestamp = useCurrentTimestamp;
    }

    public EmbedBuilder ToBuilder()
    {
        var builder = new EmbedBuilder();

        if (!string.IsNullOrEmpty(Url?.Key))
            builder = builder.WithUrl(Url.Key);
        if (!string.IsNullOrEmpty(Title?.Key))
            builder = builder.WithTitle(Title.Key);
        if (!string.IsNullOrEmpty(Description?.Key))
            builder = builder.WithDescription(Description.Key);
        if (Author is not null)
            builder = builder.WithAuthor(Author.ToBuilder());
        if (Color is not null)
            builder = builder.WithColor(Color.Value);
        if (Footer is not null)
            builder = builder.WithFooter(Footer.ToBuilder());
        if (!string.IsNullOrEmpty(ImageUrl?.Key))
            builder = builder.WithImageUrl(ImageUrl.Key);
        if (!string.IsNullOrEmpty(ThumbnailUrl?.Key))
            builder = builder.WithThumbnailUrl(ThumbnailUrl.Key);
        if (Fields.Count > 0)
            builder = builder.WithFields(Fields.Select(f => f.ToBuilder()));
        if (Timestamp is not null)
            builder = builder.WithTimestamp(new DateTimeOffset(Timestamp.Value.ToUniversalTime()));
        if (UseCurrentTimestamp)
            builder = builder.WithCurrentTimestamp();

        return builder;
    }

    public bool IsValidEmbed()
    {
        try
        {
            ToBuilder().Build();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static DiscordMessageEmbed FromEmbed(IEmbed embed)
    {
        return new DiscordMessageEmbed(
            embed.Url,
            embed.Title,
            embed.Description,
            DiscordMessageEmbedAuthor.FromEmbed(embed.Author),
            embed.Color?.RawValue,
            DiscordMessageEmbedFooter.FromEmbed(embed.Footer),
            string.IsNullOrEmpty(embed.Image?.Url) ? null : new(embed.Image.Value.Url, []),
            string.IsNullOrEmpty(embed.Thumbnail?.Url) ? null : new(embed.Thumbnail.Value.Url, []),
            embed.Fields.Select(DiscordMessageEmbedField.FromEmbed),
            embed.Timestamp?.DateTime,
            false
        );
    }
}
