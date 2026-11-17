namespace GrillBot.Contracts.AuditLog.Responses.Detail;

public class ArchivationDetail
{
    public string ProcessName { get; set; } = null!;
    public int TotalRecordsCount { get; set; }
    public long JsonSize { get; set; }
    public long ArchiveSize { get; set; }
    public string? Details { get; set; }
    public int? DeletedFilesCount { get; set; }
    public long? DeletedFilesSize { get; set; }
}
