using GrillBot.Core.RabbitMQ.V2.Messages;

namespace UserMeasures.Models.Events;

public class DeletePayload : IRabbitMessage
{
    public string Topic => "UserMeasures";
    public string Queue => "Delete";

    public Guid Id { get; set; }

    public DeletePayload()
    {
    }

    public DeletePayload(Guid id)
    {
        Id = id;
    }
}
