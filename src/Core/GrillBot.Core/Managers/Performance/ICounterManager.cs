namespace GrillBot.Core.Managers.Performance;

public interface ICounterManager
{
    CounterItem Create(string section);
    Dictionary<string, int> GetActiveCounters();
    List<CounterStats> GetStatistics();
}
