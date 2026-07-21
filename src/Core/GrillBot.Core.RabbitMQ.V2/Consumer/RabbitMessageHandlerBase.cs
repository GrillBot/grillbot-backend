using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Core.RabbitMQ.V2.Messages;
using GrillBot.Core.RabbitMQ.V2.Serialization.Json;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

#pragma warning disable S3267
namespace GrillBot.Core.RabbitMQ.V2.Consumer;

public abstract class RabbitMessageHandlerBase<TMessage>(
    ILoggerFactory _loggerFactory
) : IRabbitMessageHandler where TMessage : class, IRabbitMessage, new()
{
    public virtual string TopicName => new TMessage().Topic;
    public virtual string QueueName => new TMessage().Queue;

    protected ILogger Logger
        => _loggerFactory.CreateLogger(GetType());

    public async Task<RabbitConsumptionResult> HandleAsync(JsonNode? message, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
    {
        try
        {
            var deserializedMessage = message?.Deserialize<TMessage>(JsonRabbitMessageSerializer.SerializerOptions);
            if (deserializedMessage is null)
                return RabbitConsumptionResult.Reject;

            var currentUser = new CurrentUserProvider(headers);
            return await HandleInternalAsync(deserializedMessage, currentUser, headers, cancellationToken);
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "An error occured while deserializing message.");
            return RabbitConsumptionResult.Reject;
        }
    }

    protected abstract Task<RabbitConsumptionResult> HandleInternalAsync(
        TMessage message,
        ICurrentUserProvider currentUser,
        Dictionary<string, string> headers,
        CancellationToken cancellationToken = default
    );

    public virtual bool HandleException(Exception exception) => false;

    public Task<RabbitConsumptionResult> HandleRawMessageAsync(string rawMessage, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
    {
        if (Logger.IsEnabled(LogLevel.Warning))
            Logger.LogWarning("{Message}", rawMessage);

        foreach (var header in headers)
        {
            if (Logger.IsEnabled(LogLevel.Warning))
                Logger.LogWarning("Header({Key}): {Value}", header.Key, header.Value);
        }

        return Task.FromResult(RabbitConsumptionResult.Success);
    }
}
