using System.Diagnostics.Metrics;

namespace GrillBot.Core.Metrics.Components;

public class TelemetryCounterContainer(string name, string? description = null) : TelemetryCollectorComponent(name, null, description)
{
    private readonly Dictionary<string, TelemetryCounter> _counters = [];
    private readonly Lock _lock = new();

    public void Increment(string name, Dictionary<string, object?>? tags = null, string? description = null)
    {
        using (_lock.EnterScope())
        {
            if (!_counters.TryGetValue(name, out var gauge))
            {
                gauge = new(name, tags, description);
                _counters.Add(name, gauge);
            }

            gauge.Increment();
        }
    }

    public IEnumerable<Measurement<long>> Get()
    {
        using (_lock.EnterScope())
        {
            return [.. _counters.Values.Select(o => o.Get())];
        }
    }

    public override Instrument CreateInstrument(Meter meter)
        => meter.CreateObservableCounter(Name, Get, description: Description);
}
