using GrillBot.Core.RabbitMQ.V2.Messages;

namespace GrillBot.Core.RabbitMQ.V2.Publisher;

public interface IRabbitPublisher
{
    Task PublishAsync<T>(string topic, string queue, T data, Dictionary<string, string?>? headers = null, CancellationToken cancellationToken = default);
    Task PublishAsync<T>(string topic, string queue, List<T> data, Dictionary<string, string?>? headers = null, CancellationToken cancellationToken = default);

    Task PublishAsync<T>(T message, Dictionary<string, string?>? headers = null, CancellationToken cancellationToken = default) where T : IRabbitMessage;
    Task PublishAsync<T>(List<T> messages, Dictionary<string, string?>? headers = null, CancellationToken cancellationToken = default) where T : IRabbitMessage;
}
