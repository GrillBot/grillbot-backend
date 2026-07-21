namespace GrillBot.Contracts.AuditLog.Events.Create;

public class UserLeftRequest : UserJoinedRequest
{
    public string UserId { get; set; } = null!;

    public UserLeftRequest()
    {
    }

    public UserLeftRequest(string userId, int memberCount) : base(memberCount)
    {
        UserId = userId;
    }
}
