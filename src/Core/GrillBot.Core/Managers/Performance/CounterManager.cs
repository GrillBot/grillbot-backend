
namespace GrillBot.Core.Managers.Performance;

public class CounterManager : ICounterManager
{
    private List<CounterItem> ActiveCounters { get; } = [];
    private Dictionary<string, CounterStats> Stats { get; } = [];
    private readonly Lock _lock = new();

    public CounterItem Create(string section)
    {
        using (_lock.EnterScope())
        {
            var item = new CounterItem(this, section);

            ActiveCounters.Add(item);
            return item;
        }
    }

    public void Complete(CounterItem item)
    {
        using (_lock.EnterScope())
        {
            ActiveCounters.RemoveAll(o => o.Section == item.Section && o.Id == item.Id);

            if (!Stats.ContainsKey(item.Section))
                Stats.Add(item.Section, new CounterStats { Section = item.Section });

            var now = DateTime.Now;
            Stats[item.Section].Increment((now - item.StartAt).TotalMilliseconds);
        }
    }

    public Dictionary<string, int> GetActiveCounters()
    {
        using (_lock.EnterScope())
        {
            return ActiveCounters
                .GroupBy(o => o.Section)
                .Select(o => new { o.Key, Count = o.Count() })
                .OrderByDescending(o => o.Count)
                .ThenBy(o => o.Key)
                .ToDictionary(o => o.Key, o => o.Count);
        }
    }

    public List<CounterStats> GetStatistics()
    {
        using (_lock.EnterScope())
        {
            return [.. Stats.Values.Select(o => o.Clone())];
        }
    }
}
