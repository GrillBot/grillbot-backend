namespace GrillBot.Contracts.UserManagement.Events;

/// <summary>
/// Broker addresses for the user management bounded context, including the infraction records.
/// </summary>
public static class MessagingConstants
{
    public const string Exchange = "user-management";

    public static class RoutingKeys
    {
        public const string NicknameChanged = "nickname-changed";
        public const string MemberWarning = "member-warning";
        public const string Timeout = "timeout";
        public const string Unverify = "unverify";
        public const string UnverifyModify = "unverify-modify";
    }
}
