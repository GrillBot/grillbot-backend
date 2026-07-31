namespace GrillBot.Contracts.UserManagement.Events;

/// <summary>
/// Broker addresses for the user management bounded context.
/// </summary>
public static class MessagingConstants
{
    public const string Exchange = "user-management";

    public static class RoutingKeys
    {
        public const string NicknameChanged = "nickname-changed";
    }
}
