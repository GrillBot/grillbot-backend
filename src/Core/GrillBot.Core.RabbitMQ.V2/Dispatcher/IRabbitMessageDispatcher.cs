using GrillBot.Core.RabbitMQ.V2.Consumer;

namespace GrillBot.Core.RabbitMQ.V2.Dispatcher;

public interface IRabbitMessageDispatcher
{
    Task<RabbitConsumptionResult> HandleMessageAsync(IRabbitMessageHandler handler, byte[] body, Dictionary<string, string> headers, CancellationToken cancellationToken = default);
}
