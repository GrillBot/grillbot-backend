using Discord;

namespace GrillBot.Contracts.AuditLog.Events.Create;

public class EmbedRequest
{
    public string? Title { get; set; }
    public string Type { get; set; } = null!;
    public string? ImageInfo { get; set; }
    public string? VideoInfo { get; set; }
    public string? AuthorName { get; set; }
    public bool ContainsFooter { get; set; }
    public string? ProviderName { get; set; }
    public string? ThumbnailInfo { get; set; }
    public List<EmbedFieldBuilder> Fields { get; set; } = [];

    public EmbedRequest()
    {
    }

    public EmbedRequest(
        string? title,
        string type,
        string? imageInfo,
        string? videoInfo,
        string? authorName,
        bool containsFooter,
        string? providerName,
        string? thumbnailInfo,
        List<EmbedFieldBuilder> fields
    )
    {
        Title = title;
        Type = type;
        ImageInfo = imageInfo;
        VideoInfo = videoInfo;
        AuthorName = authorName;
        ContainsFooter = containsFooter;
        ProviderName = providerName;
        ThumbnailInfo = thumbnailInfo;
        Fields = fields;
    }
}
