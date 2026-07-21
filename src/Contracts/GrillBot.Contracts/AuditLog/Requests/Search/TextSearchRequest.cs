using GrillBot.Core.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.AuditLog.Requests.Search;

public class TextSearchRequest : IDictionaryObject, IAdvancedSearchRequest
{
    public string? Text { get; set; }

    [StringLength(100)]
    public string? SourceAppName { get; set; }

    [StringLength(512)]
    public string? Source { get; set; }

    public bool IsSet()
        => !string.IsNullOrEmpty(Text) || !string.IsNullOrEmpty(SourceAppName) || !string.IsNullOrEmpty(Source);

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(Text), Text },
            { nameof(SourceAppName), SourceAppName },
            { nameof(Source), Source }
        };
    }
}
