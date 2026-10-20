using GrillBot.Contracts.UserManagement.Responses;
using GrillBot.Services.Common.EntityFramework.Helpers;
using Microsoft.EntityFrameworkCore;
using UserManagementService.Core.Entity;
using UserManagementService.Core.Entity.Infractions;

namespace UserManagementService.Actions.Infractions;

/// <summary>
/// Per-guild infraction counts of one user. Shared by the infractions summary and the user info.
/// </summary>
public static class InfractionCounter
{
    public static async Task<UserInfractionsSummary> ComputeSummaryAsync(
        ContextHelper<UserManagementContext> contextHelper,
        ulong userId,
        ulong? guildId,
        CancellationToken cancellationToken
    )
    {
        return new UserInfractionsSummary(
            await ComputeCountAsync<WarningInfraction>(contextHelper, userId, guildId, cancellationToken),
            await ComputeCountAsync<UnverifyInfraction>(contextHelper, userId, guildId, cancellationToken),
            await ComputeCountAsync<TimeoutInfraction>(contextHelper, userId, guildId, cancellationToken)
        );
    }

    private static async Task<Dictionary<string, int>> ComputeCountAsync<TEntity>(
        ContextHelper<UserManagementContext> contextHelper,
        ulong userId,
        ulong? guildId,
        CancellationToken cancellationToken
    ) where TEntity : InfractionBase
    {
        var baseQuery = contextHelper.DbContext.Set<TEntity>()
            .Where(o => o.UserId == userId)
            .AsNoTracking();

        if (guildId is not null)
            baseQuery = baseQuery.Where(o => o.GuildId == guildId.Value);

        var query = baseQuery
            .GroupBy(o => o.GuildId)
            .Select(o => new
            {
                o.Key,
                Count = o.Count()
            });

        var counts = await contextHelper.ReadToDictionaryAsync(query, o => o.Key, o => o.Count, cancellationToken);
        return counts.ToDictionary(o => o.Key.ToString(), o => o.Value);
    }
}
