namespace GrillBot.Contracts.AuditLog.Events.Create;

public class MessageEditedRequest
{
    public string JumpUrl { get; set; } = null!;
    public string? ContentBefore { get; set; }
    public string ContentAfter { get; set; } = null!;

    public MessageEditedRequest()
    {
    }

    public MessageEditedRequest(string jumpUrl, string? contentBefore, string contentAfter)
    {
        JumpUrl = jumpUrl;
        ContentBefore = contentBefore;
        ContentAfter = contentAfter;
    }
}
