using RabbitMQ.Client;

namespace GrillBot.Core.RabbitMQ.V2.Factory;

public class RabbitChannelFactory : IRabbitChannelFactory
{
    public async Task<IChannel> CreateChannelAsync(IConnection connection, string topic, string? queue = null, CancellationToken cancellationToken = default)
    {
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        var deadLetterTopicName = $"{topic}.dead_letter";
        var deadLetterQueueName = $"{topic}.{queue}.dead_letter";

        await channel.ExchangeDeclareAsync(topic, ExchangeType.Topic, true, cancellationToken: cancellationToken); // Basic topic.
        await channel.ExchangeDeclareAsync(deadLetterTopicName, ExchangeType.Topic, true, cancellationToken: cancellationToken);

        if (string.IsNullOrEmpty(queue))
            return channel;

        var queueArgs = new Dictionary<string, object?>
        {
             { "x-dead-letter-exchange", deadLetterTopicName }
        };

        // Basic queue
        var queueName = $"{topic}.{queue}";
        await channel.QueueDeclareAsync(queueName, true, false, false, queueArgs, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(queueName, topic, queue, cancellationToken: cancellationToken);

        // Dead letter queues
        await channel.QueueDeclareAsync(deadLetterQueueName, true, false, false, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(deadLetterQueueName, deadLetterTopicName, deadLetterQueueName, cancellationToken: cancellationToken);

        return channel;
    }
}
