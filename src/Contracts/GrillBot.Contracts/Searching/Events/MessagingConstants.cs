namespace GrillBot.Contracts.Searching.Events;

/// <summary>
/// Broker addresses for the searching bounded context.
/// </summary>
public static class MessagingConstants
{
    public const string Exchange = "searching";

    public static class RoutingKeys
    {
        public const string CreateSearchItem = "create-search-item";
        public const string Synchronization = "synchronization";
    }
}
