using AuditLogService.Core.Entity;
using AuditLogService.Core.Archivation;
using GrillBot.Contracts.AuditLog.Responses.Info;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Managers.Performance;
using GrillBot.Services.Common.Infrastructure.Api;
using Microsoft.EntityFrameworkCore;

namespace AuditLogService.Actions.Info;

public class GetStatusInfoAction(
    AuditLogServiceContext context,
    AuditLogRetention _retention,
    ICounterManager counterManager
) : ApiAction<AuditLogServiceContext>(counterManager, context)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var result = new StatusInfo
        {
            ItemsToArchive = await ReadItemsToArchiveCountAsync()
        };

        return ApiResult.Ok(result);
    }

    private async Task<int> ReadItemsToArchiveCountAsync()
    {
        var query = _retention.QueryExpired(DbContext);

        return await ContextHelper.ReadCountAsync(query);
    }
}
