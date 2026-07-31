using GrillBot.Core.AsyncMessaging.Errors;
using GrillBot.Core.AsyncMessaging.HealthChecks;
using GrillBot.Core.AsyncMessaging.Options;
using GrillBot.Core.AsyncMessaging.Topology;
using JasperFx.CodeGeneration.Model;
using JasperFx.RuntimeCompiler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.RabbitMQ;

namespace GrillBot.Core.AsyncMessaging;

public static class AsyncMessagingExtensions
{
    /// <summary>
    /// Configures Wolverine over RabbitMQ for one application: it can publish every message in
    /// <see cref="MessageTopology"/> and it listens to the single queue named in the
    /// "AsyncMessaging" section.
    ///
    /// Broker credentials keep coming from the "RabbitMQ" section - only the Wolverine tuning
    /// is new configuration.
    /// </summary>
    /// <param name="runningAssembly">
    /// The assembly Wolverine scans for handlers. Must be the running application, never this
    /// shared package.
    /// </param>
    /// <param name="configureWolverine">
    /// Applied after the topology and before the error policies, for anything a single
    /// application needs on top.
    /// </param>
    /// <param name="configureQueue">Applied to this application's listener queue.</param>
    public static IHostApplicationBuilder UseAsyncMessaging(
        this IHostApplicationBuilder builder,
        Assembly runningAssembly,
        Action<WolverineOptions>? configureWolverine = null,
        Action<IRabbitMqQueue>? configureQueue = null
    )
    {
        builder.UseWolverine(opts =>
            Configure(opts, builder.Configuration, builder.Environment, runningAssembly, configureWolverine, configureQueue));

        builder.Services.AddAsyncMessagingHealthCheck();
        return builder;
    }

    /// <summary>
    /// The same configuration for hosts that are still built the Startup way and only reach
    /// the service collection.
    /// </summary>
    /// <inheritdoc cref="UseAsyncMessaging(IHostApplicationBuilder, Assembly, Action{WolverineOptions}, Action{IRabbitMqQueue})" />
    public static IServiceCollection AddAsyncMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        Assembly runningAssembly,
        Action<WolverineOptions>? configureWolverine = null,
        Action<IRabbitMqQueue>? configureQueue = null
    )
    {
        services.AddWolverine(opts =>
            Configure(opts, configuration, environment, runningAssembly, configureWolverine, configureQueue));

        return services.AddAsyncMessagingHealthCheck();
    }

    /// <summary>
    /// Registers the broker health check under the same "RabbitMQ" name the old messaging
    /// layer used, so existing health dashboards keep working.
    /// </summary>
    public static IServiceCollection AddAsyncMessagingHealthCheck(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck<AsyncMessagingHealthCheck>("RabbitMQ");

        return services;
    }

    private static void Configure(
        WolverineOptions opts,
        IConfiguration configuration,
        IHostEnvironment environment,
        Assembly runningAssembly,
        Action<WolverineOptions>? configureWolverine,
        Action<IRabbitMqQueue>? configureQueue
    )
    {
        var options = ReadOptions(configuration);

        // Also the suffix of the OpenTelemetry meter, which is named "Wolverine:<ServiceName>".
        opts.ServiceName = options.QueueName;
        opts.Discovery.IncludeAssembly(runningAssembly);

        opts
            .UseRabbitMq(rabbit =>
            {
                rabbit.HostName = configuration["RabbitMQ:Hostname"]!;
                rabbit.UserName = configuration["RabbitMQ:Username"]!;
                rabbit.Password = configuration["RabbitMQ:Password"]!;
            })
            .AutoProvision()
            .EnableEnhancedDeadLettering()
            .EnableWolverineControlQueues();

        opts.Durability.DeadLetterQueueExpirationEnabled = true;
        opts.Durability.DeadLetterQueueExpiration = options.DeadLetterQueueExpiration;
        opts.Durability.Mode = environment.IsDevelopment() ? DurabilityMode.Solo : DurabilityMode.Balanced;
        opts.DefaultExecutionTimeout = options.DefaultExecutionTimeout;

        opts.Services.AddRuntimeCompilation();
        opts.ServiceLocationPolicy = ServiceLocationPolicy.AlwaysAllowed;

        MessageTopology.ApplyPublishing(opts);

        opts
            .ListenToRabbitQueue(options.QueueName, queue =>
            {
                configureQueue?.Invoke(queue);
                queue.PurgeOnStartup = options.PurgeOnStartup;
            })
            .MaximumParallelMessages(options.MaximumParallelMessages)
            .ListenerCount(options.ListenerCount);

        MessageTopology.ApplyBindings(opts, options.QueueName);

        configureWolverine?.Invoke(opts);

        // A handler that asks to be retried gets the cooldowns but no error notification -
        // it already said the failure was expected.
        opts.OnException<TransientMessageException>()
            .RetryWithCooldown(options.RetryCooldowns);

        opts.OnAnyException()
            .RetryWithCooldown(options.RetryCooldowns)
            .And(WolverineErrorNotifier.NotifyAsync, "Discord Error Notification")
            .Then.MoveToErrorQueue();
    }

    private static AsyncMessagingOptions ReadOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection("AsyncMessaging");
        if (!section.Exists())
            throw new InvalidOperationException("Missing the \"AsyncMessaging\" configuration section.");

        var options = section.Get<AsyncMessagingOptions>()!;

        if (string.IsNullOrEmpty(options.QueueName))
            throw new InvalidOperationException("\"AsyncMessaging:QueueName\" is required - it names the queue this application listens to.");

        if (!Queues.All.Contains(options.QueueName))
            throw new InvalidOperationException($"\"AsyncMessaging:QueueName\" is \"{options.QueueName}\", which is not a known deployable. See docker/deployables.json.");

        return options;
    }
}
