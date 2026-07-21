using System.Diagnostics.Metrics;

namespace GrillBot.Core.Metrics.Components;

public class TelemetryGauge(
    string name,
    Dictionary<string, object?>? tags = null,
    string? description = null
) : TelemetryCollectorComponent(name, tags, description)
{
    private long _value;

    public void Set(long value)
        => WithLock(() => _value = value);

    public Measurement<long> Get()
        => WithLock(() => new Measurement<long>(_value, Tags));

    public override Instrument CreateInstrument(Meter meter)
        => meter.CreateObservableGauge(Name, Get, description: Description);
}
