using GrillBot.Core.Infrastructure;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.AuditLog.Requests.Search;

public class UserIdSearchRequest : IDictionaryObject, IAdvancedSearchRequest
{
    [DiscordId]
    [StringLength(32)]
    public string? UserId { get; set; }

    public bool IsSet()
        => !string.IsNullOrEmpty(UserId);

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(UserId), UserId }
        };
    }
}
