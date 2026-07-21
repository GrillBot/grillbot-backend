using GrillBot.Core.Managers.Performance;

namespace GrillBot.Core.Services.Diagnostics.Models;

public class DiagnosticInfo
{
    public long UsedMemory { get; set; }
    public long Uptime { get; set; }
    public long RequestsCount { get; set; }
    public long CpuTime { get; set; }
    public DateTime MeasuredFrom { get; set; }
    public List<RequestStatistics> Endpoints { get; set; } = [];
    public Dictionary<string, long>? DatabaseStatistics { get; set; }
    public List<OperationStatItem> Operations { get; set; } = [];
}
