using AuditLog.Enums;

namespace AuditLog.Models.Response.Detail;

public class Detail
{
    public LogType Type { get; set; }
    public object? Data { get; set; }
}
