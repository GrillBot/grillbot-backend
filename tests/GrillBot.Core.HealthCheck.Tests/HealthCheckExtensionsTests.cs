using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Core.HealthCheck.Tests;

[TestClass]
public class HealthCheckExtensionsTests
{
    [TestMethod]
    public void AddHealthChecks_ReturnsBuilder()
    {
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();

        Assert.IsNotNull(builder);
    }

    [TestMethod]
    public void MapHealthChecks_ReturnsHealthyResponse()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHealthChecks();

        var app = builder.Build();
        var healthBuilder = app.MapHealthChecks();

        Assert.IsNotNull(healthBuilder);
        Assert.IsNotNull(healthBuilder.SimpleEndpointBuilder);
        Assert.IsNotNull(healthBuilder.DetailedEndpointBuilder);
    }
}