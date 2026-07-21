namespace GrillBot.Core.Managers.Performance;

public class OperationStatItem
{
    public string Section { get; set; } = null!;
    public long Count { get; set; }
    public long TotalTime { get; set; }

    public long AverageTime => Count == 0 ? 0 : TotalTime / Count;

    public List<OperationStatItem> ChildItems { get; set; } = [];
}
