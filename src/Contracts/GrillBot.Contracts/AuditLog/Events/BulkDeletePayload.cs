using GrillBot.Core.RabbitMQ.V2.Messages;

namespace GrillBot.Contracts.AuditLog.Events;

public class BulkDeletePayload : IRabbitMessage
{
    public string Topic => "AuditLog";
    public string Queue => "BulkDelete";

    public List<Guid> Ids { get; set; } = [];

    public BulkDeletePayload()
    {
    }

    public BulkDeletePayload(List<Guid> ids)
    {
        Ids = ids;
    }

    public BulkDeletePayload(Guid id) : this([id])
    {
    }
}
