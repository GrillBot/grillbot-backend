#pragma warning disable IDE0290 // Use primary constructor
namespace GrillBot.Core.Managers.Performance;

public sealed class CounterItem : IDisposable
{
    public Guid Id { get; set; }
    public string Section { get; }
    public DateTime StartAt { get; }

    private CounterManager CounterManager { get; }

    public CounterItem(CounterManager counterManager, string section)
    {
        Id = Guid.NewGuid();
        StartAt = DateTime.Now;
        CounterManager = counterManager;
        Section = section;
    }

    public void Dispose()
        => CounterManager.Complete(this);
}
