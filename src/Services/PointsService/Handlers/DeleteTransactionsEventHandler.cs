using GrillBot.Core.Infrastructure.Auth;
using PointsService.Core.Entity;
using PointsService.Handlers.Abstractions;
using GrillBot.Contracts.Points.Events;

namespace PointsService.Handlers;

public class DeleteTransactionsEventHandler(
    IServiceProvider serviceProvider
) : BasePointsEvent(serviceProvider)
{
    public async Task HandleAsync(DeleteTransactionsPayload message, CancellationToken cancellationToken)
    {
        var transactions = await ReadTransactionsAsync(message);
        if (transactions.Count == 0)
            return;

        DbContext.RemoveRange(transactions);
        await ContextHelper.SaveChangesAsync(cancellationToken);
        await EnqueueUserForRecalculationAsync(transactions);
        return;
    }

    private async Task<List<Transaction>> ReadTransactionsAsync(DeleteTransactionsPayload payload)
    {
        var query = DbContext.Transactions
            .Where(o => o.GuildId == payload.GuildId && o.MergedCount == 0 && o.MessageId == payload.MessageId);
        if (!string.IsNullOrEmpty(payload.ReactionId))
            query = query.Where(o => o.ReactionId == payload.ReactionId);

        return await ContextHelper.ReadEntitiesAsync(query);
    }

    private Task EnqueueUserForRecalculationAsync(List<Transaction> transactions)
    {
        var users = transactions
            .GroupBy(o => new { o.GuildId, o.UserId })
            .Select(o => (o.Key.GuildId, o.Key.UserId));

        return EnqueueUsersForRecalculationAsync(users).AsTask();
    }
}
