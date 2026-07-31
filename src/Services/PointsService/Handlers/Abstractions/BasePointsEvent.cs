using GrillBot.Core.AsyncMessaging.Extensions;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using PointsService.Core.Entity;
using GrillBot.Contracts.Points.Events;

namespace PointsService.Handlers.Abstractions;

public abstract class BasePointsEvent(
    IServiceProvider serviceProvider
) : EventHandlerBaseWithDb<PointsServiceContext>(serviceProvider)
{
    protected async Task<User> FindOrCreateUserAsync(string guildId, string userId)
    {
        var userQuery = DbContext.Users.Where(o => o.GuildId == guildId && o.Id == userId);
        var entity = await ContextHelper.ReadFirstOrDefaultEntityAsync(userQuery);

        if (entity is null)
        {
            entity = new User
            {
                Id = userId,
                GuildId = guildId,
                IsUser = true,
                PointsPosition = int.MaxValue
            };

            await DbContext.AddAsync(entity);
        }

        return entity;
    }

    protected ValueTask EnqueueUserForRecalculationAsync(string guildId, string userId)
        => Publisher.PublishAsync(new UserRecalculationPayload(guildId, userId));

    protected ValueTask EnqueueUsersForRecalculationAsync(IEnumerable<(string guildId, string userId)> users)
        => Publisher.PublishAllAsync(users.Select(o => new UserRecalculationPayload(o.guildId, o.userId)));
}
