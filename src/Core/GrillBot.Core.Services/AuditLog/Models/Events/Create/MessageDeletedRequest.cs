namespace AuditLog.Models.Events.Create;

public class MessageDeletedRequest
{
    public string AuthorId { get; set; } = null!;
    public DateTime MessageCreatedAt { get; set; }
    public string? Content { get; set; }
    public List<EmbedRequest> Embeds { get; set; } = [];

    public MessageDeletedRequest()
    {
    }

    public MessageDeletedRequest(string authorId, DateTime messageCreatedAt, string? content, List<EmbedRequest> embeds)
    {
        AuthorId = authorId;
        MessageCreatedAt = messageCreatedAt;
        Content = content;
        Embeds = embeds;
    }
}
