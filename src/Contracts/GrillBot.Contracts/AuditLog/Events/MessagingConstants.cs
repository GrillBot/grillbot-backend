namespace GrillBot.Contracts.AuditLog.Events;

/// <summary>
/// Broker addresses for the audit log bounded context. Routing keys are the only
/// place a message name is written down - the events themselves stay transport-free.
/// </summary>
public static class MessagingConstants
{
    public const string Exchange = "audit-log";

    public static class RoutingKeys
    {
        public const string BulkDelete = "bulk-delete";
        public const string CreateItems = "create-items";
        public const string FileDelete = "file-delete";
        public const string UserRecalculation = "user-recalculation";
    }
}
