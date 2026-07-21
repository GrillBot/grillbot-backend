using GrillBot.Core.RabbitMQ.V2.Messages;
using AuditLog.Enums;

namespace AuditLog.Models.Events.Recalculation;

public class RecalculationMessage : IRabbitMessage
{
    public string Topic => "AuditLog";
    public string Queue => "Recalculation";

    public LogType Type { get; set; }
    public InteractionRecalculationData? Interaction { get; set; }
    public ApiRecalculationData? Api { get; set; }
    public JobRecalculationData? Job { get; set; }

    public RecalculationMessage()
    {
    }

    public RecalculationMessage(LogType type, InteractionRecalculationData? interaction = null, ApiRecalculationData? api = null, JobRecalculationData? job = null)
    {
        Type = type;
        Interaction = interaction;
        Api = api;
        Job = job;
    }

    public override string ToString()
    {
        var item = Interaction?.ToString() ?? Api?.ToString() ?? Job?.ToString();
        return $"{Type} ({item})";
    }
}
