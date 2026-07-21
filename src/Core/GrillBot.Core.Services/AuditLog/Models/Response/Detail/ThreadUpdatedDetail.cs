using GrillBot.Core.Models;

namespace AuditLog.Models.Response.Detail;

public class ThreadUpdatedDetail
{
    public Diff<List<string>>? Tags { get; set; } = new();
}
