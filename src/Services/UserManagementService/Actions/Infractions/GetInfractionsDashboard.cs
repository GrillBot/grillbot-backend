using GrillBot.Contracts.UserManagement.Responses;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Managers.Performance;
using GrillBot.Services.Common.Infrastructure.Api;
using Microsoft.EntityFrameworkCore;
using UserManagementService.Core.Entity;
using UserManagementService.Core.Entity.Infractions;
using UserManagementService.Models.Infractions;

namespace UserManagementService.Actions.Infractions;

public class GetInfractionsDashboard(
    UserManagementContext dbContext,
    ICounterManager counterManager
) : ApiAction<UserManagementContext>(counterManager, dbContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var unverifies = await ReadRowsAsync<UnverifyInfraction>("Unverify");
        var warnings = await ReadRowsAsync<WarningInfraction>("Warning");
        var timeouts = await ReadRowsAsync<TimeoutInfraction>("Timeout");
        var items = unverifies.Concat(warnings).Concat(timeouts);

        var result = items
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(10)
            .Select(o => new InfractionDashboardRow
            {
                Type = o.Type,
                UserId = o.UserId
            })
            .ToList();

        return ApiResult.Ok(result);
    }

    private async Task<List<InternalDashboardRow>> ReadRowsAsync<TEntity>(string type) where TEntity : InfractionBase
    {
        var query = DbContext.Set<TEntity>()
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(10)
            .Select(o => new { o.CreatedAtUtc, o.UserId });

        var rows = await ContextHelper.ReadEntitiesAsync(query);
        return rows
            .ConvertAll(o => new InternalDashboardRow
            {
                CreatedAtUtc = o.CreatedAtUtc,
                UserId = o.UserId.ToString(),
                Type = type
            })
;
    }
}
