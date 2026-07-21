using GrillBot.Contracts.AuditLog.Enums;

namespace GrillBot.Contracts.AuditLog.Responses.Detail;

public class Detail
{
    public LogType Type { get; set; }
    public object? Data { get; set; }
}
