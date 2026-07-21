namespace GrillBot.Contracts.AuditLog.Responses.Statistics;

public class FileExtensionStatistic
{
    public string Extension { get; set; } = null!;
    public long Size { get; set; }
    public long Count { get; set; }
}
