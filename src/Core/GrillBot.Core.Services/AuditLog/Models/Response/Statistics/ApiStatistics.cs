namespace AuditLog.Models.Response.Statistics;

public class ApiStatistics
{
    public Dictionary<string, long> ByDateInternalApi { get; set; } = [];
    public Dictionary<string, long> ByDatePublicApi { get; set; } = [];
    public List<StatisticItem> Endpoints { get; set; } = [];
}
