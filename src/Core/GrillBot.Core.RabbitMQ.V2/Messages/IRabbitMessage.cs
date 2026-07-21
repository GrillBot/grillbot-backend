namespace GrillBot.Core.RabbitMQ.V2.Messages;

public interface IRabbitMessage
{
    string Topic { get; }
    string Queue { get; }
}
