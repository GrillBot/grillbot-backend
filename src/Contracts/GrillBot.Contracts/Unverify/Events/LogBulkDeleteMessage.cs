using System.Text.Json.Serialization;

namespace GrillBot.Contracts.Unverify.Events;

[method: JsonConstructor]
public sealed record LogBulkDeleteMessage(List<Guid> Ids)
{
    public LogBulkDeleteMessage(Guid id) : this([id])
    {
    }
}
