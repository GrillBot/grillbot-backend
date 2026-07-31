namespace GrillBot.Contracts.UserMeasures.Events;

/// <summary>
/// Broker addresses for the user measures bounded context.
/// </summary>
public static class MessagingConstants
{
    public const string Exchange = "user-measures";

    public static class RoutingKeys
    {
        public const string MemberWarning = "member-warning";
        public const string Timeout = "timeout";
        public const string Unverify = "unverify";
        public const string UnverifyModify = "unverify-modify";
    }
}
