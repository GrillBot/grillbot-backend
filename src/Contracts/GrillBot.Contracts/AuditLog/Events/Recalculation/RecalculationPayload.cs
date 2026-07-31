using GrillBot.Contracts.AuditLog.Enums;

namespace GrillBot.Contracts.AuditLog.Events.Recalculation;

/// <summary>
/// Published and consumed by the audit log service itself to recalculate user
/// statistics off the request thread.
/// </summary>
public sealed record RecalculationPayload(
    LogType Type,
    InteractionRecalculationData? Interaction = null,
    ApiRecalculationData? Api = null,
    JobRecalculationData? Job = null,
    int FilesCount = 0
)
{
    public override string ToString()
    {
        var item = Interaction?.ToString() ?? Api?.ToString() ?? Job?.ToString();

        if (string.IsNullOrEmpty(item) && FilesCount > 0)
            item = $"Files:{FilesCount}";

        return $"{Type} ({item})";
    }
}
