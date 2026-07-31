namespace GrillBot.Contracts.Invite.Events;

/// <summary>
/// Broker addresses for the invite bounded context.
/// </summary>
public static class MessagingConstants
{
    public const string Exchange = "invite";

    public static class RoutingKeys
    {
        public const string InviteCreated = "invite-created";
        public const string SynchronizeGuildInvites = "synchronize-guild-invites";
        public const string UserJoined = "user-joined";
    }
}
