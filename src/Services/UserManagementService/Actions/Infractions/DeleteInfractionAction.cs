using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Managers.Performance;
using GrillBot.Services.Common.Infrastructure.Api;
using GrillBot.Contracts.UserManagement.Requests;
using UserManagementService.Core.Entity;
using UserManagementService.Core.Entity.Infractions;

namespace UserManagementService.Actions.Infractions;

public class DeleteInfractionAction(
    ICounterManager counterManager,
    UserManagementContext dbContext
) : ApiAction<UserManagementContext>(counterManager, dbContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var request = GetParameter<DeleteInfractionRequest>(0);
        var itemFound = false;

        if (request.Id.HasValue)
        {
            itemFound |= await DeleteEntityAsync<WarningInfraction>(request.Id.Value);
            itemFound |= await DeleteEntityAsync<UnverifyInfraction>(request.Id.Value);
            itemFound |= await DeleteEntityAsync<TimeoutInfraction>(request.Id.Value);
        }

        if (!string.IsNullOrEmpty(request.ExternalIdType) && request.ExternalId.HasValue)
        {
            switch (request.ExternalIdType.ToLower())
            {
                case "unverify":
                    itemFound |= await DeleteEntityAsync(DbContext.Unverifies.Where(o => o.LogSetId == request.ExternalId.Value));
                    break;
                case "timeout":
                    itemFound |= await DeleteEntityAsync(DbContext.Timeouts.Where(o => o.ExternalId == request.ExternalId.Value));
                    break;
                default:
                    return new ApiResult(StatusCodes.Status406NotAcceptable);
            }
        }

        return itemFound ? ApiResult.Ok() : ApiResult.NotFound();
    }

    private Task<bool> DeleteEntityAsync<TEntity>(Guid id) where TEntity : InfractionBase
    {
        var query = DbContext.Set<TEntity>().Where(o => o.Id == id);
        return DeleteEntityAsync(query);
    }

    private async Task<bool> DeleteEntityAsync<TEntity>(IQueryable<TEntity> query) where TEntity : InfractionBase
    {
        var item = await ContextHelper.ReadFirstOrDefaultEntityAsync(query);
        if (item is null)
            return false;

        DbContext.Remove(item);
        await ContextHelper.SaveChangesAsync();
        return true;
    }
}
