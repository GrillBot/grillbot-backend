using GrillBot.Core.Infrastructure;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.AuditLog.Requests.Search;

public class MessageDeletedSearchRequest : IDictionaryObject, IAdvancedSearchRequest
{
    public bool? ContainsEmbed { get; set; }
    public string? ContentContains { get; set; }

    [DiscordId]
    [StringLength(32)]
    public string? AuthorId { get; set; }

    public bool IsSet()
        => ContainsEmbed is not null || !string.IsNullOrEmpty(ContentContains) || !string.IsNullOrEmpty(AuthorId);

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(ContainsEmbed), ContainsEmbed?.ToString() },
            { nameof(ContentContains), ContentContains },
            { nameof(AuthorId), AuthorId }
        };
    }
}
