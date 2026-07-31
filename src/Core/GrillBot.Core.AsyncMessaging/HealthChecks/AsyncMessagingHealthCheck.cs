using Microsoft.Extensions.Diagnostics.HealthChecks;
using Wolverine.Runtime;
using Wolverine.Transports;

namespace GrillBot.Core.AsyncMessaging.HealthChecks;

/// <summary>
/// Reports the broker as healthy once Wolverine's runtime is up, which means the connection
/// was established, the topology was provisioned and the listener for this application's
/// queue is running.
/// </summary>
public class AsyncMessagingHealthCheck(IWolverineRuntime _runtime) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var listeners = _runtime.Endpoints.ActiveListeners().ToList();

        if (listeners.Count == 0)
            return Task.FromResult(HealthCheckResult.Unhealthy("No message listener is running."));

        var failed = listeners.Where(o => o.Status == ListeningStatus.Stopped).Select(o => o.Uri.ToString()).ToList();

        return Task.FromResult(failed.Count > 0
            ? HealthCheckResult.Unhealthy($"Stopped listeners: {string.Join(", ", failed)}")
            : HealthCheckResult.Healthy($"{listeners.Count} listener(s) running."));
    }
}
