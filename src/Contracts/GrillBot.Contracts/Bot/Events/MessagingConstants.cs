namespace GrillBot.Contracts.Bot.Events;

/// <summary>
/// Broker addresses for everything consumed by the Discord bot itself.
/// </summary>
public static class MessagingConstants
{
    public const string Exchange = "grillbot";

    public static class RoutingKeys
    {
        public const string CreatedMessage = "created-message";
        public const string EditMessage = "edit-message";
        public const string ErrorNotification = "error-notification";
        public const string SendMessage = "send-message";
    }
}
