using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Managers.Performance;
using Microsoft.EntityFrameworkCore;
using PointsService.Core;
using PointsService.Core.Entity;
using GrillBot.Contracts.Points;
using Wolverine;

namespace PointsService.Actions;

public class GetCurrentPointsStatusAction(
    ICounterManager counterManager,
    PointsServiceContext dbContext,
    IMessageBus publisher
) : ApiAction(counterManager, dbContext, publisher)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = (string)Parameters[0]!;
        var userId = (string)Parameters[1]!;

        var leaderboardItem = await FindLeaderboardItemAsync(guildId, userId);
        var result = new PointsStatus
        {
            MonthBack = leaderboardItem.MonthBack,
            Today = leaderboardItem.Today,
            Total = leaderboardItem.Total,
            YearBack = leaderboardItem.YearBack
        };

        return ApiResult.Ok(result);
    }

    private async Task<LeaderboardItem> FindLeaderboardItemAsync(string guildId, string userId)
    {
        var query = DbContext.Leaderboard.AsNoTracking()
            .Where(o => o.GuildId == guildId && o.UserId == userId);

        var result = await ContextHelper.ReadFirstOrDefaultEntityAsync(query);
        return result ?? new LeaderboardItem();
    }
}
