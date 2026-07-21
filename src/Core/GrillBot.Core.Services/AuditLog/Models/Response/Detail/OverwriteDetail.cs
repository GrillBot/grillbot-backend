using Discord;

namespace AuditLog.Models.Response.Detail;

public class OverwriteDetail
{
    public string TargetId { get; set; } = null!;
    public PermissionTarget TargetType { get; set; }

    public List<string> Allow { get; set; } = [];
    public List<string> Deny { get; set; } = [];
}
