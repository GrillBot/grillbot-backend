using GrillBot.Core.Metrics.Collectors;
using GrillBot.Core.Metrics.Initializer;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace GrillBot.Core.Metrics.Services;

public class TelemetryService(
    Meter _meter,
    IEnumerable<ITelemetryCollector> _collectors,
    IEnumerable<TelemetryInitializer> _initializers
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _meter.CreateObservableGauge("grillbot_workingset", () => Process.GetCurrentProcess().WorkingSet64, description: "Current working set of the service");
        _meter.CreateObservableGauge("grillbot_starttime_seconds", () => new DateTimeOffset(Process.GetCurrentProcess().StartTime).ToUniversalTime().ToUnixTimeSeconds(), description: "Start time of service.");

        foreach (var component in _collectors.SelectMany(o => o.GetComponents()))
            component.CreateInstrument(_meter);

        foreach (var initializer in _initializers)
            await initializer.ExecuteAsync(stoppingToken);
    }
}
