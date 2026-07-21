namespace AuditLog.Models.Events.Create;

public class UnbanRequest
{
    public string UserId { get; set; } = null!;

    public UnbanRequest()
    {
    }

    public UnbanRequest(string userId)
    {
        UserId = userId;
    }
}
