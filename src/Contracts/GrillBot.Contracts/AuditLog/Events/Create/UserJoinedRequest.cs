namespace GrillBot.Contracts.AuditLog.Events.Create;

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
