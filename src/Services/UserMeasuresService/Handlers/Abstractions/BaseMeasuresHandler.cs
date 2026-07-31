using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using UserMeasuresService.Core.Entity;

namespace UserMeasuresService.Handlers.Abstractions;

public abstract class BaseMeasuresHandler(
    IServiceProvider serviceProvider
) : EventHandlerBaseWithDb<UserMeasuresContext>(serviceProvider)
{
    protected async Task SaveEntityAsync<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        if (entity is null)
            return;

        if (entity.IsNew)
        {
            entity.Id = Guid.NewGuid();
            await DbContext.AddAsync(entity);
        }

        await ContextHelper.SaveChangesAsync();
    }
}
