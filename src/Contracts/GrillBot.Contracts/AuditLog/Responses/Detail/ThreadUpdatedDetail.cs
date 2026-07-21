using GrillBot.Core.Models;

namespace GrillBot.Contracts.AuditLog.Responses.Detail;

public class ThreadUpdatedDetail
{
    public Diff<List<string>>? Tags { get; set; } = new();
}
