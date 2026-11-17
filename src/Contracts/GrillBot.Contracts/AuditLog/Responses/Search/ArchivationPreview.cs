namespace GrillBot.Contracts.AuditLog.Responses.Search;

public class ArchivationPreview
{
    public string ProcessName { get; set; } = null!;
    public int TotalRecordsCount { get; set; }
}
