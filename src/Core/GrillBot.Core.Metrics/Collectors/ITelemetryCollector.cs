using GrillBot.Core.Metrics.Components;

namespace GrillBot.Core.Metrics.Collectors;

public interface ITelemetryCollector
{
    IEnumerable<TelemetryCollectorComponent> GetComponents();
}
