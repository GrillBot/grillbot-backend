using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Managers.Performance;
using PointsService.Core;
using PointsService.Core.Entity;
using Wolverine;

namespace PointsService.Actions;

public class GetLeaderboardCountAction(
    ICounterManager counterManager,
    PointsServiceContext dbContext,
    IMessageBus publisher
) : ApiAction(counterManager, dbContext, publisher)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = (string)Parameters[0]!;
        var query = DbContext.Leaderboard.Where(o => o.GuildId == guildId);
        var result = await ContextHelper.ReadCountAsync(query);

        return ApiResult.Ok(result);
    }
}
