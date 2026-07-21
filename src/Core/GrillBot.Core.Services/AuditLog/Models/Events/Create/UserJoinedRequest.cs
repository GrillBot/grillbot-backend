namespace AuditLog.Models.Events.Create;

public class UserJoinedRequest
{
    public int MemberCount { get; set; }

    public UserJoinedRequest()
    {
    }

    public UserJoinedRequest(int memberCount)
    {
        MemberCount = memberCount;
    }
}
