using GrillBot.Core.Metrics;
using GrillBot.Core.RabbitMQ.V2.Consumer;
using GrillBot.Core.RabbitMQ.V2.Dispatcher;
using GrillBot.Core.RabbitMQ.V2.Factory;
using GrillBot.Core.RabbitMQ.V2.HealthChecks;
using GrillBot.Core.RabbitMQ.V2.Options;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.RabbitMQ.V2.Serialization;
using GrillBot.Core.RabbitMQ.V2.Serialization.Json;
using GrillBot.Core.RabbitMQ.V2.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GrillBot.Core.RabbitMQ.V2;

public static class RabbitExtensions
{
    private static readonly Type _handlerInterfaceType = typeof(IRabbitMessageHandler);

    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        if (!configuration.GetSection("RabbitMQ").Exists())
            return services;

        services.Configure<RabbitOptions>(configuration.GetSection("RabbitMQ"));
        services.AddSingleton<IRabbitConnectionFactory, RabbitConnectionFactory>();
        services.AddSingleton<IRabbitMessageSerializer, JsonRabbitMessageSerializer>();
        services.AddSingleton<IRabbitMessageDispatcher, RabbitJsonMessageDispatcher>();
        services.AddSingleton<IRabbitChannelFactory, RabbitChannelFactory>();
        services.AddScoped<IRabbitPublisher, RabbitPublisher>();
        services.AddHostedService<RabbitConsumerService>();

        // Telemetry
        services.AddTelemetryCollector<RabbitTelemetryCollector>();

        services
            .AddHealthChecks()
            .AddCheck<RabbitHealthCheck>("RabbitMQ");

        return services;
    }

    public static IServiceCollection AddRabbitConsumer(this IServiceCollection services, Type handlerType)
    {
        if (handlerType.GetInterface(_handlerInterfaceType.Name) is null)
            throw new ArgumentException($"Type {handlerType.Name} does not implement interface {_handlerInterfaceType.Name}");

        return services.AddScoped(typeof(IRabbitMessageHandler), handlerType);
    }

    public static IServiceCollection AddRabbitConsumer<TImplementation>(this IServiceCollection services) where TImplementation : IRabbitMessageHandler
        => AddRabbitConsumer(services, typeof(TImplementation));

    public static IServiceCollection AddRabbitConsumersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var typesQuery = assembly
            .GetTypes()
            .Where(o => o.IsClass && !o.IsAbstract && o.GetInterface(_handlerInterfaceType.Name) is not null);

        foreach (var handler in typesQuery)
            services.AddRabbitConsumer(handler);
        return services;
    }
}
