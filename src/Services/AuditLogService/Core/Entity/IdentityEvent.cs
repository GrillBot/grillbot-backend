using System.ComponentModel.DataAnnotations;
using GrillBot.Contracts.AuditLog.Enums;

namespace AuditLogService.Core.Entity;

/// <summary>An identity / access event (<see cref="LogType.Identity"/>). The user is <see cref="LogItem.UserId"/>.</summary>
public class IdentityEvent : ChildEntityBase
{
    public IdentityEventType EventType { get; set; }

    [StringLength(32)]
    public string Channel { get; set; } = null!;

    [StringLength(1024)]
    public string? Reason { get; set; }

    [StringLength(64)]
    public string? IpAddress { get; set; }
}
