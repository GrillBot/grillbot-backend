using System.Reflection;

namespace GrillBot.Core.RabbitMQ.V2.Messages;

public class RabbitErrorMessage : IRabbitMessage
{
    public string Topic => "GrillBot";
    public string Queue => "RabbitErrors";

    public string AssemblyName { get; set; } = null!;
    public string TopicName { get; set; } = null!;
    public string QueueName { get; set; } = null!;
    public string? RawMessage { get; set; }
    public Dictionary<string, string> Headers { get; set; } = [];
    public string HandlerType { get; set; } = null!;
    public string Exception { get; set; } = null!;

    public RabbitErrorMessage()
    {
    }

    public RabbitErrorMessage(
        Exception ex,
        string topic,
        string queue,
        Dictionary<string, string> headers,
        string handlerType,
        string? rawMessage
    )
    {
        AssemblyName = Assembly.GetEntryAssembly()!.GetName().Name!;
        TopicName = topic;
        QueueName = queue;
        Headers = headers;
        HandlerType = handlerType;
        Exception = ex.ToString();
        RawMessage = rawMessage;
    }
}
