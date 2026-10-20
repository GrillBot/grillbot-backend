using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Managers.Performance;
using GrillBot.Services.Common.Infrastructure.Api;
using UserManagementService.Core.Entity;
using UserManagementService.Core.Entity.Infractions;

namespace UserManagementService.Actions.Infractions;

public class GetInfractionsCountOfGuild(UserManagementContext dbContext, ICounterManager counterManager) : ApiAction<UserManagementContext>(counterManager, dbContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = ulong.Parse((string)Parameters[0]!);
        var unverifies = await ComputeRowsAsync<UnverifyInfraction>(guildId);
        var warnings = await ComputeRowsAsync<WarningInfraction>(guildId);
        var timeouts = await ComputeRowsAsync<TimeoutInfraction>(guildId);
        var result = unverifies + warnings + timeouts;

        return ApiResult.Ok(result);
    }

    private async Task<int> ComputeRowsAsync<TEntity>(ulong guildId) where TEntity : InfractionBase
        => await ContextHelper.ReadCountAsync(DbContext.Set<TEntity>().Where(o => o.GuildId == guildId));
}
