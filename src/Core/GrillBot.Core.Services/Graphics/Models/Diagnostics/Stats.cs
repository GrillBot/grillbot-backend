using GrillBot.Core.Services.Diagnostics.Models;

namespace Graphics.Models.Diagnostics;

public class Stats
{
    public int RequestsCount { get; set; }
    public DateTime MeasuredFrom { get; set; }
    public List<RequestStatistics> Endpoints { get; set; } = [];
    public long CpuTime { get; set; }
}
