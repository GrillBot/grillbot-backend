using GrillBot.Core.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace AuditLog.Models.Request.Search;

public class TextSearchRequest : IDictionaryObject
{
    public string? Text { get; set; }

    [StringLength(100)]
    public string? SourceAppName { get; set; }

    [StringLength(512)]
    public string? Source { get; set; }

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
