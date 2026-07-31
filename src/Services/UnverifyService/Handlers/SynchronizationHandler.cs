using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Contracts.Unverify.Events;
using GrillBot.Contracts.Unverify.Events.Users;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using UnverifyService.Core.Entity;

namespace UnverifyService.Handlers;

public class SynchronizationHandler(IServiceProvider serviceProvider)
    : EventHandlerBaseWithDb<UnverifyContext>(serviceProvider)
{
    public async Task HandleAsync(SynchronizationMessage message, CancellationToken cancellationToken)
    {
        foreach (var user in message.Users)
            await SynchronizeUserAsync(user, cancellationToken);

        await DbContext.SaveChangesAsync(cancellationToken);
        await Publisher.PublishAsync(new RecalculateMetricsMessage());

        return;
    }

    private async Task SynchronizeUserAsync(UserSyncItem syncItem, CancellationToken cancellationToken = default)
    {
        var userQuery = DbContext.Users.Where(o => o.Id == syncItem.UserId);
        var entity = await ContextHelper.ReadFirstOrDefaultEntityAsync(userQuery, cancellationToken);

        if (entity is null)
        {
            entity = new User
            {
                Id = syncItem.UserId,
            };

            await DbContext.AddAsync(entity, cancellationToken);
        }

        entity.IsBot = syncItem.IsBot;
        entity.Language = syncItem.UserLanguage;
    }
}
