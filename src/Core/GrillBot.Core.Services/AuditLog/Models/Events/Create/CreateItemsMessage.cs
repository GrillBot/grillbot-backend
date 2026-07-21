using GrillBot.Core.RabbitMQ.V2.Messages;

namespace AuditLog.Models.Events.Create;

public class CreateItemsMessage : IRabbitMessage
{
    public string Topic => "AuditLog";
    public string Queue => "CreateItems";

    public List<LogRequest> Items { get; set; } = [];

    public CreateItemsMessage()
    {
    }

    public CreateItemsMessage(List<LogRequest> items)
    {
        Items = items;
    }

    public CreateItemsMessage(LogRequest item) : this([item])
    {
    }
}
