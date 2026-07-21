namespace GrillBot.Contracts.AuditLog.Responses.Detail;

public class MessageEditedDetail
{
    public string ContentBefore { get; set; } = null!;
    public string ContentAfter { get; set; } = null!;
    public string JumpLink { get; set; } = null!;
}
