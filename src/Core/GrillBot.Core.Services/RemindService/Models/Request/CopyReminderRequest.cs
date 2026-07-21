using GrillBot.Core.Infrastructure;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace RemindService.Models.Request;

public class CopyReminderRequest : IDictionaryObject
{
    public long RemindId { get; set; }

    [DiscordId]
    [StringLength(32)]
    public string ToUserId { get; set; } = null!;

    [StringLength(32)]
    public string Language { get; set; } = null!;

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(RemindId), RemindId.ToString() },
            { nameof(ToUserId), ToUserId },
            { nameof(Language), Language }
        };
    }
}
