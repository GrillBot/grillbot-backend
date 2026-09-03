using GrillBot.Core.Configuration;
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
        var redisConfig = configuration.GetSection(RedisOptions.SectionName);
        if (!redisConfig.Exists())
            return services;

        // Declaring the section is what makes Redis mandatory for this host, so the endpoint
        // has to be filled in before the host is allowed to finish starting.
        services.AddValidatedOptions<RedisOptions>(configuration, RedisOptions.SectionName);

        services.AddScoped(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>().GetSection(RedisOptions.SectionName)!;
            return ConnectionMultiplexer.Connect(CreateRedisOptions(config));
        });

        services.AddScoped(provider => provider.GetRequiredService<ConnectionMultiplexer>().GetDatabase());

        services.AddScoped(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>().GetSection(RedisOptions.SectionName)!;
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
