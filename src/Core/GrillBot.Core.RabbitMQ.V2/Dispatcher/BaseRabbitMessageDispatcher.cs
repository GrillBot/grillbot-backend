using GrillBot.Core.RabbitMQ.V2.Consumer;
using GrillBot.Core.RabbitMQ.V2.Serialization;

namespace GrillBot.Core.RabbitMQ.V2.Dispatcher;

public class BaseRabbitMessageDispatcher(
    IRabbitMessageSerializer _serializer
) : IRabbitMessageDispatcher
{
    public virtual async Task<RabbitConsumptionResult> HandleMessageAsync(IRabbitMessageHandler handler, byte[] body, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
    {
        var rawMessage = await _serializer.DeserializeToStringAsync(body, cancellationToken: cancellationToken);
        return await handler.HandleRawMessageAsync(rawMessage, headers, cancellationToken);
    }

    protected TSerializer GetSerializer<TSerializer>() where TSerializer : IRabbitMessageSerializer
        => (TSerializer)_serializer;
}
