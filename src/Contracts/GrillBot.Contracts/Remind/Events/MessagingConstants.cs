namespace GrillBot.Contracts.Remind.Events;

/// <summary>
/// Broker addresses for the remind bounded context.
/// </summary>
public static class MessagingConstants
{
    public const string Exchange = "remind";

    public static class RoutingKeys
    {
        public const string RemindMessageNotify = "remind-message-notify";
        public const string SendRemindNotification = "send-remind-notification";
    }
}
