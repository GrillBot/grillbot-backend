namespace GrillBot.Contracts.Unverify.Events;

/// <summary>
/// Broker addresses for the unverify bounded context.
/// </summary>
public static class MessagingConstants
{
    public const string Exchange = "unverify";

    public static class RoutingKeys
    {
        public const string GuildUserLeft = "guild-user-left";
        public const string LogBulkDelete = "log-bulk-delete";
        public const string RecalculateMetrics = "recalculate-metrics";
        public const string RecoverAccess = "recover-access";
        public const string SetUnverify = "set-unverify";
        public const string Synchronization = "synchronization";
    }
}
