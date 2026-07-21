using RabbitMQ.Client;

namespace GrillBot.Core.RabbitMQ.V2.Factory;

public interface IRabbitConnectionFactory
{
    Task<IConnection> CreateAsync(CancellationToken cancellationToken = default);
}
