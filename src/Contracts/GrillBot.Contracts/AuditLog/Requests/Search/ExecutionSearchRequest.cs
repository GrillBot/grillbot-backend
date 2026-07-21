using GrillBot.Core.Infrastructure;

namespace GrillBot.Contracts.AuditLog.Requests.Search;

public class ExecutionSearchRequest : IDictionaryObject, IAdvancedSearchRequest
{
    public string? ActionName { get; set; }
    public bool? Success { get; set; }
    public int? DurationFrom { get; set; }
    public int? DurationTo { get; set; }

    public bool IsSet()
        => !string.IsNullOrEmpty(ActionName) || Success is not null || DurationFrom is not null || DurationTo is not null;

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(ActionName), ActionName },
            { nameof(Success), Success?.ToString() },
            { nameof(DurationFrom), DurationFrom?.ToString() },
            { nameof(DurationTo), DurationTo?.ToString() }
        };
    }
}
