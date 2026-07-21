using GrillBot.Core.Extensions;
using GrillBot.Core.HealthCheck.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Core.HealthCheck;

public static class HealthCheckExtensions
{
    public static IHealthChecksBuilder AddHealthChecks(this IServiceCollection services)
    {
        return HealthCheckServiceCollectionExtensions.AddHealthChecks(services);
    }

    public static HealthCheckEndpointRouteConfiguration MapHealthChecks(this IEndpointRouteBuilder endpoints, string pattern = "/health")
    {
        // Simple endpoint for public health check. Does not return detailed information.
        var simpleEndpoint = HealthCheckEndpointRouteBuilderExtensions.MapHealthChecks(endpoints, pattern).RequireUserAgent();

        var options = new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = HealthCheckWriter.WriteResponseAsync
        };

        // Detailed health check endpoint that returns JSON response. Only for certified purposes.
        var fullEndpoint = endpoints.MapHealthChecks($"{pattern}/json", options).RequireUserAgent();
        return new(simpleEndpoint, fullEndpoint);
    }
}
