namespace AuditLogService.Core.Options;

public class AppOptions
{
    public ArchivationOptions Archivation { get; set; } = new();
}
