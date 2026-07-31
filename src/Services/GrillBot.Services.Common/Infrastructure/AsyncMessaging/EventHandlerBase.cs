using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Contracts.AuditLog.Events.Create;
using GrillBot.Core.Managers.Performance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Wolverine;

namespace GrillBot.Services.Common.Infrastructure.AsyncMessaging;

/// <summary>
/// The shared plumbing every message handler in a service wants: a message bus to cascade
/// further events, performance counters and a logger.
///
/// It deliberately says nothing about the message type or the transport. Wolverine finds a
/// handler by its "Handler" suffix and its Handle/HandleAsync method, so a subclass only
/// declares the message it takes.
/// </summary>
public abstract class EventHandlerBase
{
    protected ICounterManager CounterManager { get; }
    protected IMessageBus Publisher { get; }
    protected IServiceProvider ServiceProvider { get; }
    protected ILogger Logger { get; }

    /// <summary>Prefix for every counter this handler creates.</summary>
    protected string CounterKey { get; }

    protected EventHandlerBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        CounterManager = serviceProvider.GetRequiredService<ICounterManager>();
        Publisher = serviceProvider.GetRequiredService<IMessageBus>();
        Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());

        CounterKey = $"AsyncMessaging.{GetType().Name}";
    }

    protected CounterItem CreateCounter(string operation)
        => CounterManager.Create($"{CounterKey}.{operation}");

    /// <summary>
    /// Records that a message arrived without the Authorization header a handler needed.
    /// </summary>
    protected ValueTask NotifyUnauthorizedExecution(string messageType)
    {
        var request = new LogRequest(LogType.Warning, DateTime.UtcNow)
        {
            LogMessage = new LogMessageRequest(
                $"Unauthorized usage of event handler. Missing Authorization token. Message: {messageType}",
                Assembly.GetEntryAssembly()!.GetName().Name!,
                GetType().Name
            )
        };

        return Publisher.PublishAsync(new CreateItemsMessage(request));
    }
}
