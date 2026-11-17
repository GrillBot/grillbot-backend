using System.Linq.Expressions;
using AuditLogService.Core.Entity;
using AuditLogService.Core.Options;
using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Services.Common.Archivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuditLogService.Core.Archivation;

/// <summary>
/// Retention policy of the audit log. The conditions live here (they must translate to SQL);
/// the durations come from <see cref="ArchivationOptions.RetentionMonths"/>. The first matching
/// rule wins, so conditional rules go before broader rules for the same types.
/// Soft-deleted records are always expired.
/// </summary>
/// <remarks>
/// Conditions on child tables are correlated <c>EXISTS</c> subqueries over the context's sets, not
/// navigations. A navigation becomes a <c>LEFT JOIN</c> of the whole child table (ApiRequests,
/// LogMessages, ...) into every query; a subquery runs only for the records whose type already matched.
/// Every condition starts with the type test for the same reason.
/// </remarks>
public class AuditLogRetention(IOptionsMonitor<AppOptions> _options)
{
    public const string GatewayReconnect = "GatewayReconnect";
    public const string UserLeftBan = "UserLeftBan";
    public const string ApiNonV2 = "ApiNonV2";
    public const string ChannelAndRoleChanges = "ChannelAndRoleChanges";
    public const string OverwritesAndIdentity = "OverwritesAndIdentity";
    public const string Unban = "Unban";

    /// <summary>How many leading characters of a log message the <see cref="GatewayReconnect"/> rule searches.</summary>
    private const int GatewayReconnectSearchLength = 1000;

    /// <summary>Rule names in priority order. Each needs a value in <see cref="ArchivationOptions.RetentionMonths"/>.</summary>
    public static readonly IReadOnlyList<string> RuleNames =
        [GatewayReconnect, UserLeftBan, ApiNonV2, ChannelAndRoleChanges, OverwritesAndIdentity, Unban];

    private static IEnumerable<(string Name, Expression<Func<LogItem, bool>> Match)> CreateRules(AuditLogServiceContext context)
    {
        IQueryable<LogMessage> logMessages = context.LogMessages;
        IQueryable<UserLeft> userLeftItems = context.UserLeftItems;
        IQueryable<ApiRequest> apiRequests = context.ApiRequests;

        // Only the start of the message is searched. Messages are Discord.Net's "Source: message Exception.ToString()",
        // and .NET writes the exception type and every inner exception before the stack traces, so the name is near
        // the start. The prefix lets PostgreSQL decompress only the first TOAST chunk of a message instead of the
        // whole stack trace (~180 KB on average). Checked on production data: same matches as the full-text search.
        yield return (GatewayReconnect, o => (o.Type == LogType.Info || o.Type == LogType.Warning || o.Type == LogType.Error)
            && logMessages.Any(m => m.LogItemId == o.Id && m.Message.Substring(0, GatewayReconnectSearchLength).Contains("GatewayReconnectException")));
        yield return (UserLeftBan, o => o.Type == LogType.UserLeft && userLeftItems.Any(u => u.LogItemId == o.Id && u.IsBan));
        yield return (ApiNonV2, o => o.Type == LogType.Api && apiRequests.Any(a => a.LogItemId == o.Id && a.ApiGroupName != "V2"));
        yield return (ChannelAndRoleChanges, o => o.Type == LogType.ChannelDeleted || o.Type == LogType.ChannelUpdated || o.Type == LogType.ChannelCreated
            || o.Type == LogType.ThreadDeleted || o.Type == LogType.EmoteDeleted || o.Type == LogType.RoleDeleted || o.Type == LogType.MemberRoleUpdated);
        yield return (OverwritesAndIdentity, o => o.Type == LogType.OverwriteCreated || o.Type == LogType.OverwriteDeleted || o.Type == LogType.OverwriteUpdated
            || o.Type == LogType.Identity);
        yield return (Unban, o => o.Type == LogType.Unban);
    }

    /// <summary>The policy bound to <paramref name="context"/>; its predicate may only be used in queries of that context.</summary>
    public RetentionPolicy<LogItem> CreatePolicy(AuditLogServiceContext context)
        => CreatePolicy(_options.CurrentValue.Archivation, context);

    public static RetentionPolicy<LogItem> CreatePolicy(ArchivationOptions options, AuditLogServiceContext context)
    {
        var rules = CreateRules(context).Select(rule =>
        {
            if (!options.RetentionMonths.TryGetValue(rule.Name, out var months))
                throw new InvalidOperationException($"Missing configuration Archivation:RetentionMonths:{rule.Name}.");

            return new RetentionRule<LogItem>(rule.Name, rule.Match, RetentionPeriod.FromMonths(months));
        });

        return new RetentionPolicy<LogItem>(
            o => o.CreatedAt,
            RetentionPeriod.FromMonths(options.DefaultRetentionMonths),
            rules,
            o => o.IsDeleted
        );
    }

    /// <summary>Records whose retention expired as of now.</summary>
    public IQueryable<LogItem> QueryExpired(AuditLogServiceContext context)
        => context.LogItems.AsNoTracking().Where(CreatePolicy(context).BuildExpiredPredicate(DateTime.UtcNow));

    /// <summary>Validates the configuration at startup (missing or non-positive values).</summary>
    public static bool IsValid(AppOptions options)
    {
        var archivation = options.Archivation;

        return archivation.DefaultRetentionMonths > 0
            && RuleNames.All(name => archivation.RetentionMonths.TryGetValue(name, out var months) && months > 0)
            && archivation.MaxItemsToArchivePerRun > 0
            && archivation.ReadPageSize > 0
            && archivation.TelemetryRefreshInterval > TimeSpan.Zero;
    }
}
