using Microsoft.AspNetCore.Builder;

namespace GrillBot.Core.HealthCheck.Models;

public record HealthCheckEndpointRouteConfiguration(
    IEndpointConventionBuilder SimpleEndpointBuilder,
    IEndpointConventionBuilder DetailedEndpointBuilder
);
