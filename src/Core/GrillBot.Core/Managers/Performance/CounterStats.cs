namespace GrillBot.Core.Managers.Performance;

public class CounterStats
{
    public string Section { get; set; } = null!;
    public long TotalTime { get; set; }
    public long Count { get; set; }

    public long AverageTime => TotalTime / Count;

    public void Increment(double duration)
    {
        Count++;
        TotalTime += (long)duration;
    }

    internal CounterStats Clone()
    {
        return new CounterStats
        {
            Count = Count,
            Section = Section,
            TotalTime = TotalTime
        };
    }
}
