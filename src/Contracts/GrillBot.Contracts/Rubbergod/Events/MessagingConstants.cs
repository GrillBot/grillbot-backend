namespace GrillBot.Contracts.Rubbergod.Events;

/// <summary>
/// Broker addresses for the Rubbergod bounded context.
/// </summary>
public static class MessagingConstants
{
    public const string Exchange = "rubbergod";

    public static class RoutingKeys
    {
        public const string ClearPinCache = "clear-pin-cache";
        public const string KarmaBatch = "karma-batch";
    }
}
