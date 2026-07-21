using System.Diagnostics.Metrics;

namespace GrillBot.Core.Metrics.Components;

public abstract class TelemetryCollectorComponent(
    string name,
    Dictionary<string, object?>? tags = null,
    string? description = null
)
{
    private readonly Lock _lock = new();

    protected string Name => name;
    protected Dictionary<string, object?> Tags => tags ?? [];
    protected string? Description => description;

    protected void WithLock(Action action)
    {
        using (_lock.EnterScope())
        {
            action();
        }
    }

    protected TValue WithLock<TValue>(Func<TValue> func)
    {
        using (_lock.EnterScope())
        {
            return func();
        }
    }

    public abstract Instrument CreateInstrument(Meter meter);
}
