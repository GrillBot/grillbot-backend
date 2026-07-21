using Discord;

namespace GrillBot.Contracts.AuditLog.Responses.Search;

public class OverwritePreview
{
    public string TargetId { get; set; } = null!;
    public PermissionTarget TargetType { get; set; }
}
