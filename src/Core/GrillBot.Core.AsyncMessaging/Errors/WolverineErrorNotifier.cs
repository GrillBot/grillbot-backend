using System.Reflection;
using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Contracts.AuditLog.Events.Create;
using GrillBot.Contracts.Bot.Events.Errors;
using GrillBot.Core.Extensions;
using Wolverine;
using Wolverine.Runtime;

namespace GrillBot.Core.AsyncMessaging.Errors;

/// <summary>
/// Reports a message that exhausted its retries: a Discord embed for the humans and an
/// audit log entry for the record.
///
/// This used to be a round trip - the failing service republished a RabbitErrorMessage
/// and the bot's handler turned it into a notification. Wolverine's error policies let
/// the failing service report directly, so the message never leaves the service that
/// could not handle it.
/// </summary>
public static class WolverineErrorNotifier
{
    private const int MaxExceptionLength = 500;

    public static async ValueTask NotifyAsync(IWolverineRuntime runtime, IEnvelopeLifecycle lifecycle, Exception exception)
    {
        var envelope = lifecycle.Envelope;
        var serviceName = Assembly.GetEntryAssembly()?.GetName().Name ?? runtime.Options.ServiceName;
        var messageType = envelope?.MessageType ?? "Unknown";
        var destination = envelope?.Destination?.ToString() ?? "Unknown";

        // The policy runs outside any DI scope, so the bus is built straight from the runtime.
        var bus = new MessageBus(runtime);

        await bus.PublishAsync(new ErrorNotificationPayload(
            "Při zpracování zprávy z RabbitMQ došlo k chybě.",
            [.. BuildFields(serviceName, messageType, destination, exception)],
            null
        ));

        await bus.PublishAsync(new CreateItemsMessage(new LogRequest(LogType.Error, DateTime.UtcNow)
        {
            LogMessage = new LogMessageRequest
            {
                Message = exception.ToString(),
                Source = $"AsyncMessaging/{destination}/{messageType}",
                SourceAppName = serviceName
            }
        }));
    }

    private static IEnumerable<ErrorNotificationField> BuildFields(string serviceName, string messageType, string destination, Exception exception)
    {
        yield return new ErrorNotificationField("Služba", serviceName, true);
        yield return new ErrorNotificationField("Zpráva", messageType, true);
        yield return new ErrorNotificationField("Cíl", destination, true);
        yield return new ErrorNotificationField("Zkrácený obsah chyby", exception.ToString().Cut(MaxExceptionLength)!, false);
    }
}
