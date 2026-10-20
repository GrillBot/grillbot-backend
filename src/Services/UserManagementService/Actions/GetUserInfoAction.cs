using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Services.Common.Infrastructure.Api;
using Microsoft.EntityFrameworkCore;
using UnverifyService;
using UserManagementService.Actions.Infractions;
using UserManagementService.Core.Entity;
using GrillBot.Contracts.UserManagement.Responses;

using UnverifyServiceModels = GrillBot.Contracts.Unverify.Responses.Users;

namespace UserManagementService.Actions;

public class GetUserInfoAction(
    ICounterManager counterManager,
    UserManagementContext context,
    IServiceClientExecutor<IUnverifyServiceClient> _unverifyClient
) : ApiAction<UserManagementContext>(counterManager, context)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var userId = GetParameter<ulong>(0);
        var userData = await GetUserDataAsync(userId);
        var unverifyInfo = await GetUnverifyUserInfoAsync(userId);
        var infractions = await InfractionCounter.ComputeSummaryAsync(ContextHelper, userId, null, CancellationToken);

        if (userData.Count == 0 && unverifyInfo is null && IsEmpty(infractions))
            return ApiResult.NotFound();

        var result = new UserInfo(
            userId.ToString(),
            CreateGuildUserData(userData, unverifyInfo, infractions),
            unverifyInfo?.SelfUnverifyMinimalTime
        );

        return ApiResult.Ok(result);
    }

    private Task<List<Core.Entity.GuildUser>> GetUserDataAsync(ulong userId)
    {
        var query = DbContext.GuildUsers.AsNoTracking()
            .Include(o => o.Nicknames)
            .Where(o => o.UserId == userId);

        return ContextHelper.ReadEntitiesAsync(query, CancellationToken);
    }

    private async Task<UnverifyServiceModels.UserInfo?> GetUnverifyUserInfoAsync(ulong userId)
    {
        try
        {
            return await _unverifyClient.ExecuteRequestAsync(
                async (client, ctx) => await client.GetUserInfoAsync(userId, ctx.CancellationToken),
                CancellationToken
            );
        }
        catch (ClientNotFoundException)
        {
            return null;
        }
    }

    private static bool IsEmpty(UserInfractionsSummary infractions)
        => infractions.WarningCount.Count == 0 && infractions.UnverifyCount.Count == 0 && infractions.TimeoutCount.Count == 0;

    private static List<GrillBot.Contracts.UserManagement.Responses.GuildUser> CreateGuildUserData(
        List<Core.Entity.GuildUser> users,
        UnverifyServiceModels.UserInfo? unverifyInfo,
        UserInfractionsSummary infractions
    )
    {
        var result = new List<GrillBot.Contracts.UserManagement.Responses.GuildUser>();

        var guildIds = users.Select(o => o.GuildId.ToString())
            .Concat(unverifyInfo?.CurrentUnverifies.Keys?.ToArray() ?? [])
            .Concat(unverifyInfo?.SelfUnverifyCount.Keys?.ToArray() ?? [])
            .Concat(unverifyInfo?.UnverifyCount.Keys?.ToArray() ?? [])
            .Concat(infractions.TimeoutCount.Keys)
            .Concat(infractions.WarningCount.Keys)
            .Concat(infractions.UnverifyCount.Keys)
            .Distinct();

        foreach (var guildId in guildIds)
        {
            var guildUser = users.Find(o => o.GuildId == guildId.ToUlong());
            var currentUnverify = unverifyInfo?.CurrentUnverifies.TryGetValue(guildId, out var value) == true ? value : null;
            var selfUnverifyCount = unverifyInfo?.SelfUnverifyCount.TryGetValue(guildId, out var count) == true ? count : 0;
            var unverifyCount = infractions.UnverifyCount.GetValueOrDefault(guildId);
            var timeoutCount = infractions.TimeoutCount.GetValueOrDefault(guildId);
            var warningCount = infractions.WarningCount.GetValueOrDefault(guildId);

            result.Add(new GrillBot.Contracts.UserManagement.Responses.GuildUser(
                guildId,
                guildUser?.CurrentNickname,
                guildUser is not null ? [.. guildUser.Nicknames.OrderBy(o => o.Value).Select(o => o.Value)] : [],
                currentUnverify,
                unverifyCount,
                selfUnverifyCount,
                timeoutCount,
                warningCount
            ));
        }

        return result;
    }
}
