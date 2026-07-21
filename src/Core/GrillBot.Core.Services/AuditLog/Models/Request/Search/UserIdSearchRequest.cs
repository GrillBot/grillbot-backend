using GrillBot.Core.Infrastructure;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace AuditLog.Models.Request.Search;

public class UserIdSearchRequest : IDictionaryObject
{
    [DiscordId]
    [StringLength(32)]
    public string? UserId { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(UserId), UserId }
        };
    }
}
