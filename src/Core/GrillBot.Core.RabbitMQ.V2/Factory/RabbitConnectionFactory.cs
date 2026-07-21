using GrillBot.Core.RabbitMQ.V2.Options;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace GrillBot.Core.RabbitMQ.V2.Factory;

public class RabbitConnectionFactory(IConfiguration _configuration) : IRabbitConnectionFactory
{
    public async Task<IConnection> CreateAsync(CancellationToken cancellationToken = default)
    {
        var configuration = _configuration.GetSection("RabbitMQ");
        if (!configuration.Exists())
            throw new InvalidOperationException("Missing RabbitMQ configuration section.");

        var options = configuration.Get<RabbitOptions>()!;
        var factory = new ConnectionFactory
        {
            HostName = options.Hostname,
            Password = options.Password,
            UserName = options.Username,
            RequestedHeartbeat = options.HeartBeat
        };

        return await factory.CreateConnectionAsync(cancellationToken);
    }
}
