using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace GrillBot.Core.Redis;

public static class RedisExtensions
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddRedisDistributedCache(configuration);
    }

    public static IServiceCollection AddRedisDistributedCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfig = configuration.GetSection("Redis");
        if (!redisConfig.Exists())
            return services;

        services.AddScoped(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>().GetSection("Redis")!;
            return ConnectionMultiplexer.Connect(CreateRedisOptions(config));
        });

        services.AddScoped(provider => provider.GetRequiredService<ConnectionMultiplexer>().GetDatabase());

        services.AddScoped(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>().GetSection("Redis")!;
            var connection = provider.GetRequiredService<ConnectionMultiplexer>();

            return connection.GetServer(config["Endpoint"]!);
        });

        return services.AddStackExchangeRedisCache(opt =>
        {
            opt.Configuration = redisConfig["Endpoint"]!;
            opt.ConfigurationOptions = CreateRedisOptions(redisConfig);
        });
    }

    private static ConfigurationOptions CreateRedisOptions(IConfigurationSection redisConfig)
    {
        return new()
        {
            AbortOnConnectFail = true,
            EndPoints = { redisConfig["Endpoint"]! },
            Password = redisConfig["Password"]!
        };
    }
}
