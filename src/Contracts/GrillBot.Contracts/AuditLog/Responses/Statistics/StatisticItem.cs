namespace GrillBot.Contracts.AuditLog.Responses.Statistics;

public class StatisticItem
{
    public string Key { get; set; } = null!;
    public DateTime Last { get; set; }
    public long SuccessCount { get; set; }
    public long FailedCount { get; set; }
    public long MinDuration { get; set; }
    public long MaxDuration { get; set; }
    public long TotalDuration { get; set; }
    public long LastRunDuration { get; set; }
    // Derived from the counters above rather than transported separately - no producer ever
    // set them, so a settable pair would always have been zero on the wire.
    public int SuccessRate
    {
        get
        {
            var sum = SuccessCount + FailedCount;
            return sum == 0 ? 0 : (int)Math.Round((double)SuccessCount / sum * 100);
        }
    }

    public int AvgDuration
    {
        get
        {
            var sum = SuccessCount + FailedCount;
            return sum == 0 ? 0 : Convert.ToInt32(Math.Round(TotalDuration / (double)sum));
        }
    }
}
