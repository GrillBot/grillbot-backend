using GrillBot.Core.Managers.Performance;
using Microsoft.EntityFrameworkCore;
using PointsService.Core.Entity;
using GrillBot.Contracts.Points.Events;
using Wolverine;


namespace PointsService.Core;

public abstract class ApiAction(
    ICounterManager counterManager,
    PointsServiceContext dbContext,
    IMessageBus _publisher
) : GrillBot.Services.Common.Infrastructure.Api.ApiAction<PointsServiceContext>(counterManager, dbContext)
{
    protected IMessageBus Publisher => _publisher;

    protected async Task<User?> FindUserAsync(string guildId, string userId)
    {
        var query = DbContext.Users.Where(o => o.GuildId == guildId && o.Id == userId).AsNoTracking();
        return await ContextHelper.ReadFirstOrDefaultEntityAsync(query);
    }

    protected Task EnqueueUserForRecalculationAsync(string guildId, string userId)
        => Publisher.PublishAsync(new UserRecalculationPayload(guildId, userId)).AsTask();
}
