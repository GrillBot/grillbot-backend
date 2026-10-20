using GrillBot.Contracts.UserManagement.Requests;
using GrillBot.Contracts.UserManagement.Responses;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Models.Pagination;
using GrillBot.Services.Common.Infrastructure.Api;
using Microsoft.EntityFrameworkCore;
using UserManagementService.Core.Entity;
using UserManagementService.Core.Entity.Infractions;

namespace UserManagementService.Actions.Infractions;

public class GetInfractionsList(
    UserManagementContext dbContext,
    ICounterManager counterManager
) : ApiAction<UserManagementContext>(counterManager, dbContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (InfractionListParams)Parameters[0]!;
        var warnings = await ReadEntitiesAsync<WarningInfraction>(parameters, "Warning");
        var unverifies = await ReadEntitiesAsync<UnverifyInfraction>(parameters, "Unverify");
        var timeouts = await ReadEntitiesAsync<TimeoutInfraction>(parameters, "Timeout");
        var items = MapItems(warnings, unverifies, timeouts).OrderByDescending(o => o.CreatedAtUtc).ToList();
        var result = PaginatedResponse<InfractionItem>.Create(items, parameters.Pagination);

        return ApiResult.Ok(result);
    }

    private async Task<List<TEntity>> ReadEntitiesAsync<TEntity>(InfractionListParams parameters, string type) where TEntity : InfractionBase
    {
        if (!string.IsNullOrEmpty(parameters.Type) && parameters.Type != type)
            return [];

        var query = DbContext.Set<TEntity>().AsNoTracking();

        if (!string.IsNullOrEmpty(parameters.GuildId))
        {
            var guildId = ulong.Parse(parameters.GuildId);
            query = query.Where(o => o.GuildId == guildId);
        }

        if (!string.IsNullOrEmpty(parameters.UserId))
        {
            var userId = ulong.Parse(parameters.UserId);
            query = query.Where(o => o.UserId == userId);
        }

        if (!string.IsNullOrEmpty(parameters.ModeratorId))
        {
            var moderatorId = ulong.Parse(parameters.ModeratorId);
            query = query.Where(o => o.ModeratorId == moderatorId);
        }

        if (parameters.CreatedFrom is not null)
            query = query.Where(o => o.CreatedAtUtc >= parameters.CreatedFrom.Value);
        if (parameters.CreatedTo is not null)
            query = query.Where(o => o.CreatedAtUtc <= parameters.CreatedTo.Value);

        query = query.OrderByDescending(o => o.CreatedAtUtc);
        return await ContextHelper.ReadEntitiesAsync(query);
    }

    private static IEnumerable<InfractionItem> MapItems(List<WarningInfraction> warnings, List<UnverifyInfraction> unverifies, List<TimeoutInfraction> timeouts)
    {
        foreach (var warning in warnings.Select(TransformBase))
        {
            warning.Type = "Warning";
            yield return warning;
        }

        foreach (var unverify in unverifies)
        {
            var unverifyItem = TransformBase(unverify);

            unverifyItem.Type = "Unverify";
            unverifyItem.ValidTo = unverify.ValidTo;

            yield return unverifyItem;
        }

        foreach (var timeout in timeouts)
        {
            var timeoutItem = TransformBase(timeout);

            timeoutItem.Type = "Timeout";
            timeoutItem.ValidTo = timeout.ValidTo;

            yield return timeoutItem;
        }
    }

    private static InfractionItem TransformBase(InfractionBase baseEntity)
    {
        return new InfractionItem
        {
            ModeratorId = baseEntity.ModeratorId.ToString(),
            GuildId = baseEntity.GuildId.ToString(),
            CreatedAtUtc = baseEntity.CreatedAtUtc,
            UserId = baseEntity.UserId.ToString(),
            Reason = baseEntity.Reason,
            InfractionId = baseEntity.Id
        };
    }
}
