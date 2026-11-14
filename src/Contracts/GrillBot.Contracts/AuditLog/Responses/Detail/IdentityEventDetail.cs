using GrillBot.Contracts.AuditLog.Enums;

namespace GrillBot.Contracts.AuditLog.Responses.Detail;

public class IdentityEventDetail
{
    public IdentityEventType EventType { get; set; }
    public string Channel { get; set; } = null!;
    public string? Reason { get; set; }
    public string? IpAddress { get; set; }
}
