namespace GrillBot.Contracts.AuditLog.Responses.Statistics;

public class ApiStatistics
{
    // The two copies of this contract had disagreed on both the names and the types of these
    // two properties, so the bot was deserializing empty dictionaries. AuditLogService is the
    // producer, so its shape wins.
    public Dictionary<DateOnly, int> DailyInternalApi { get; set; } = [];
    public Dictionary<DateOnly, int> DailyPublicApi { get; set; } = [];
    public List<StatisticItem> Endpoints { get; set; } = [];
}
