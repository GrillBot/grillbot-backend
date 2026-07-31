namespace GrillBot.Contracts.Message.Events;

/// <summary>
/// Broker addresses for the message bounded context.
/// </summary>
public static class MessagingConstants
{
    public const string Exchange = "message";

    public static class RoutingKeys
    {
        public const string MessageReceived = "message-received";
        public const string Synchronization = "synchronization";
    }
}
