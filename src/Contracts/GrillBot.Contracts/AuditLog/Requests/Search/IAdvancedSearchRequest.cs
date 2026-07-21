namespace GrillBot.Contracts.AuditLog.Requests.Search;

/// <summary>
/// A per-log-type advanced filter. IsSet() reports whether the caller actually filled anything
/// in, which is what lets a search skip the log types it was not asked about.
/// </summary>
public interface IAdvancedSearchRequest
{
    bool IsSet();
}
