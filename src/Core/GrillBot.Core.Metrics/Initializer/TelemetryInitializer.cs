using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GrillBot.Core.Metrics.Initializer;

public abstract class TelemetryInitializer(IServiceProvider _serviceProvider)
{
    protected abstract Task ExecuteInternalAsync(IServiceProvider provider, CancellationToken cancellationToken = default);

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();

        scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(GetType())
            .LogInformation("Starting initialization.");

        await ExecuteInternalAsync(scope.ServiceProvider, stoppingToken);
    }
}
