using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Contracts.AuditLog.Requests.Search;

namespace AuditLogService.Models.Extensions;

/// <summary>
/// Query behaviour that used to sit on SearchRequest itself. The request is now a shared
/// contract in GrillBot.Contracts and carries data only, so how this service interprets an
/// advanced filter lives here, next to the code that reads it.
/// </summary>
public static class SearchRequestExtensions
{
    public static bool IsAdvancedFilterSet(this SearchRequest request, LogType type)
    {
        if (!request.ShowTypes.Contains(type) || request.AdvancedSearch is null)
            return false;

        IAdvancedSearchRequest? advancedSearchRequest = type switch
        {
            LogType.Info => request.AdvancedSearch.Info,
            LogType.Warning => request.AdvancedSearch.Warning,
            LogType.Error => request.AdvancedSearch.Error,
            LogType.InteractionCommand => request.AdvancedSearch.Interaction,
            LogType.JobCompleted => request.AdvancedSearch.Job,
            LogType.Api => request.AdvancedSearch.Api,
            LogType.OverwriteCreated => request.AdvancedSearch.OverwriteCreated,
            LogType.OverwriteDeleted => request.AdvancedSearch.OverwriteDeleted,
            LogType.OverwriteUpdated => request.AdvancedSearch.OverwriteUpdated,
            LogType.MemberUpdated => request.AdvancedSearch.MemberUpdated,
            LogType.MemberRoleUpdated => request.AdvancedSearch.MemberRolesUpdated,
            LogType.MessageDeleted => request.AdvancedSearch.MessageDeleted,
            LogType.MemberWarning => request.AdvancedSearch.MemberWarning,
            _ => null
        };

        return advancedSearchRequest?.IsSet() == true;
    }

    public static bool IsAnyAdvancedFilterSet(this SearchRequest request)
        => request.ShowTypes.Count > 0 && request.AdvancedSearch is not null && Array.Exists(Enum.GetValues<LogType>(), request.IsAdvancedFilterSet);
}
