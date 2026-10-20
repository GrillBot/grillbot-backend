using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using UserManagementService.Core.Entity;
using UserManagementService.Core.Entity.Infractions;

namespace UserManagementService.Handlers.Infractions;

public abstract class BaseInfractionHandler(
    IServiceProvider serviceProvider
) : EventHandlerBaseWithDb<UserManagementContext>(serviceProvider)
{
    protected async Task SaveEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : BaseEntity
    {
        if (entity is null)
            return;

        if (entity.IsNew)
        {
            entity.Id = Guid.NewGuid();
            await DbContext.AddAsync(entity, cancellationToken);
        }

        await ContextHelper.SaveChangesAsync(cancellationToken);
    }
}
