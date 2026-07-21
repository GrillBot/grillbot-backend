using System.Diagnostics.Metrics;

namespace GrillBot.Core.Metrics.Components;

public class TelemetryCounter(
    string name,
    Dictionary<string, object?>? tags = null,
    string? description = null
) : TelemetryCollectorComponent(name, tags, description)
{
    private long _value;

    public void Increment()
        => WithLock(() => _value++);

    public Measurement<long> Get()
        => WithLock(() => new Measurement<long>(_value, Tags));

    public override Instrument CreateInstrument(Meter meter)
        => meter.CreateObservableCounter(Name, Get, description: Description);
}
