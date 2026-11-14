using GrillBot.Contracts.AuditLog.Enums;

namespace GrillBot.Contracts.AuditLog.Responses.Search;

public class IdentityEventPreview
{
    public IdentityEventType EventType { get; set; }
    public string Channel { get; set; } = null!;
}
