namespace GrillBot.Contracts.Points.Events;

/// <summary>
/// Broker addresses for the points bounded context.
/// </summary>
public static class MessagingConstants
{
    public const string Exchange = "points";

    public static class RoutingKeys
    {
        public const string CreateTransaction = "create-transaction";
        public const string CreateTransactionAdmin = "create-transaction-admin";
        public const string DeleteTransactions = "delete-transactions";
        public const string Synchronization = "synchronization";
        public const string UserRecalculation = "user-recalculation";
    }
}
