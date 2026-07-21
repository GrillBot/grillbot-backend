namespace GrillBot.Contracts.AuditLog.Events.Create;

public class DeletedEmoteRequest
{
    public string EmoteId { get; set; } = null!;

    public DeletedEmoteRequest()
    {
    }

    public DeletedEmoteRequest(string emoteId)
    {
        EmoteId = emoteId;
    }
}
