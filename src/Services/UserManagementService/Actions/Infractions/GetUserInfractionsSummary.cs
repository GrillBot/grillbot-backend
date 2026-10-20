using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Managers.Performance;
using GrillBot.Services.Common.Infrastructure.Api;
using UserManagementService.Core.Entity;

namespace UserManagementService.Actions.Infractions;

public class GetUserInfractionsSummary(UserManagementContext dbContext, ICounterManager counterManager) : ApiAction<UserManagementContext>(counterManager, dbContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var userId = ulong.Parse(GetParameter<string>(0));
        var guildId = GetOptionalParameter<string>(1);

        var result = await InfractionCounter.ComputeSummaryAsync(
            ContextHelper,
            userId,
            string.IsNullOrEmpty(guildId) ? null : ulong.Parse(guildId),
            CancellationToken
        );

        return ApiResult.Ok(result);
    }
}
