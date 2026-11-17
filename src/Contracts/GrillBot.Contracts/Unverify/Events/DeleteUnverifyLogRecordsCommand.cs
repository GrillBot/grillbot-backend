using System.Text.Json.Serialization;

namespace GrillBot.Contracts.Unverify.Events;

/// <summary>
/// Deletes unverify log records (with their operations). Records of an active unverify are kept.
/// </summary>
[method: JsonConstructor]
public sealed record DeleteUnverifyLogRecordsCommand(List<Guid> Ids)
{
    public DeleteUnverifyLogRecordsCommand(Guid id) : this([id])
    {
    }
}
