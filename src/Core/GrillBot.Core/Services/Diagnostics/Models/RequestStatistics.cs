namespace GrillBot.Core.Services.Diagnostics.Models;

public class RequestStatistics
{
    public string Endpoint { get; set; } = null!;
    public DateTime LastRequestAt { get; set; }
    public int TotalTime { get; set; }
    public int LastTime { get; set; }
    public long SuccessCount { get; set; }
    public long FailedCount { get; set; }

    public long Count => SuccessCount + FailedCount;
    public int AvgTime => Count == 0 ? 0 : Convert.ToInt32(TotalTime / Count);
    public int SuccessRate => (int)Math.Round(SuccessCount / (double)Count * 100);
}
