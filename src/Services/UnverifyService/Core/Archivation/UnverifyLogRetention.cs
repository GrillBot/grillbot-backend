using GrillBot.Services.Common.Archivation;
using Microsoft.Extensions.Options;
using UnverifyService.Core.Entity.Logs;
using UnverifyService.Options;

namespace UnverifyService.Core.Archivation;

/// <summary>
/// Retention of the unverify log: one period (<see cref="ArchivationOptions.ExpirationMilestone"/>) for every record.
/// Records that still back an active unverify are never expired.
/// </summary>
public class UnverifyLogRetention(IOptionsMonitor<AppOptions> _options)
{
    public RetentionPolicy<UnverifyLogItem> CreatePolicy()
        => new(o => o.CreatedAt, RetentionPeriod.FromTimeSpan(_options.CurrentValue.Archivation.ExpirationMilestone));

    /// <summary>Records whose retention expired as of now.</summary>
    public IQueryable<UnverifyLogItem> WhereExpired(IQueryable<UnverifyLogItem> query)
        => query
            .Where(o => o.ActiveUnverify == null)
            .Where(CreatePolicy().BuildExpiredPredicate(DateTime.UtcNow));
}
