using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Managers.Performance;
using PointsService.Core;
using PointsService.Core.Entity;
using GrillBot.Contracts.Points;
using Wolverine;

namespace PointsService.Actions;

public class GetImagePointsStatusAction(
    ICounterManager counterManager,
    PointsServiceContext dbContext,
    IMessageBus publisher
) : ApiAction(counterManager, dbContext, publisher)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = (string)Parameters[0]!;
        var userId = (string)Parameters[1]!;

        var user = await FindUserAsync(guildId, userId);
        if (user is null)
            return ApiResult.NotFound();

        var result = new ImagePointsStatus
        {
            Position = user.PointsPosition,
            Points = Convert.ToInt32(user.ActivePoints)
        };

        return ApiResult.Ok(result);
    }
}
