using GrillBot.Core.RabbitMQ.V2.Messages;

namespace GrillBot.Contracts.Unverify.Events;

public class RecoverAccessMessage : IRabbitMessage
{
    public string Topic => "Unverify";
    public string Queue => "RecoverAccess";

    public long? LogNumber { get; set; }
    public Guid? LogId { get; set; }
}
