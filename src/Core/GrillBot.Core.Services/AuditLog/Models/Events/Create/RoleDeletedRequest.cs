namespace AuditLog.Models.Events.Create;

public class RoleDeletedRequest
{
    public string RoleId { get; set; } = null!;

    public RoleDeletedRequest()
    {
    }

    public RoleDeletedRequest(string roleId)
    {
        RoleId = roleId;
    }
}
