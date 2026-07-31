using System.Text.Json.Serialization;

namespace GrillBot.Contracts.AuditLog.Events.Create;

// The secondary constructor is by far the most common way this message is built,
// so the primary one has to be pointed out to the serializer explicitly.
[method: JsonConstructor]
public sealed record CreateItemsMessage(List<LogRequest> Items)
{
    public CreateItemsMessage(LogRequest item) : this([item])
    {
    }
}
