using GrillBot.Core;
using GrillBot.Core.Metrics;
using GrillBot.Services.Common;
using GrillBot.Services.Common.EntityFramework.Extensions;
using GrillBot.Services.Common.Providers;
using GrillBot.Services.Common.Telemetry.Database.Initializers;
using IdentityService.Core.Entity;
using IdentityService.Infrastructure;
using IdentityService.Options;
using System.Reflection;

var application = await ServiceBuilder.CreateWebAppAsync<AppOptions>(
    Assembly.GetExecutingAssembly(),
    args,
    (services, configuration) =>
    {
        var connectionString = configuration.GetConnectionString("Default")!;

        services.AddPostgresDatabaseContext<IdentityContext>(connectionString);
        services.AddStatisticsProvider<DefaultStatisticsProvider<IdentityContext>>();
        services.AddTelemetryInitializer<DefaultDatabaseInitializer<IdentityContext>>();
        services.AddIdentityServer(configuration);
    },
    configureHealthChecks: (healthCheckBuilder, configuration) =>
    {
        var connectionString = configuration.GetConnectionString("Default")!;
        healthCheckBuilder.AddNpgSql(connectionString);
    },
    preRunInitialization: async (app, provider) =>
    {
        await app.InitDatabaseAsync<IdentityContext>();
        await provider.GetRequiredService<ClientRegistrar>().SynchronizeAsync();
    },
    // OpenIddict handles the token, discovery and JWKS endpoints inside the authentication middleware.
    configureMiddleware: app => app.UseAuthentication()
);

await application.RunAsync();
