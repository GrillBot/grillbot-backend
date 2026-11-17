using System.Linq.Expressions;
using AuditLogService.Core.Entity;
using AuditLogService.Core.Options;
using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Services.Common.Archivation;
using Microsoft.Extensions.Options;

namespace AuditLogService.Core.Archivation;

/// <summary>
/// Retention policy of the audit log. The conditions live here (they must translate to SQL);
/// the durations come from <see cref="ArchivationOptions.RetentionMonths"/>. The first matching
/// rule wins, so conditional rules go before broader rules for the same types.
/// Soft-deleted records are always expired.
/// </summary>
public class AuditLogRetention(IOptionsMonitor<AppOptions> _options)
{
    public const string GatewayReconnect = "GatewayReconnect";
    public const string UserLeftBan = "UserLeftBan";
    public const string ApiNonV2 = "ApiNonV2";
    public const string ChannelAndRoleChanges = "ChannelAndRoleChanges";
    public const string OverwritesAndIdentity = "OverwritesAndIdentity";
    public const string Unban = "Unban";

    public static readonly IReadOnlyList<(string Name, Expression<Func<LogItem, bool>> Match)> Rules =
    [
        (GatewayReconnect, o => (o.Type == LogType.Info || o.Type == LogType.Warning || o.Type == LogType.Error)
            && o.LogMessage != null && o.LogMessage.Message.Contains("GatewayReconnectException")),
        (UserLeftBan, o => o.Type == LogType.UserLeft && o.UserLeft != null && o.UserLeft.IsBan),
        (ApiNonV2, o => o.Type == LogType.Api && o.ApiRequest != null && o.ApiRequest.ApiGroupName != "V2"),
        (ChannelAndRoleChanges, o => o.Type == LogType.ChannelDeleted || o.Type == LogType.ChannelUpdated || o.Type == LogType.ChannelCreated
            || o.Type == LogType.ThreadDeleted || o.Type == LogType.EmoteDeleted || o.Type == LogType.RoleDeleted || o.Type == LogType.MemberRoleUpdated),
        (OverwritesAndIdentity, o => o.Type == LogType.OverwriteCreated || o.Type == LogType.OverwriteDeleted || o.Type == LogType.OverwriteUpdated
            || o.Type == LogType.Identity),
        (Unban, o => o.Type == LogType.Unban)
    ];

    public RetentionPolicy<LogItem> CreatePolicy()
        => CreatePolicy(_options.CurrentValue.Archivation);

    public static RetentionPolicy<LogItem> CreatePolicy(ArchivationOptions options)
    {
        var rules = Rules.Select(rule =>
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
    public IQueryable<LogItem> WhereExpired(IQueryable<LogItem> query)
        => query.Where(CreatePolicy().BuildExpiredPredicate(DateTime.UtcNow));

    /// <summary>Validates the configuration at startup (missing or non-positive retention).</summary>
    public static bool IsValid(AppOptions options)
    {
        try
        {
            CreatePolicy(options.Archivation);
            return options.Archivation.MaxItemsToArchivePerRun > 0 && options.Archivation.ReadPageSize > 0;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return false;
        }
    }
}
