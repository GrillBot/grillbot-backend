using GrillBot.Core.Infrastructure;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.Remind.Requests;

public class CancelReminderRequest : IDictionaryObject
{
    public int RemindId { get; set; }

    /// <summary>
    /// ID of user who executes cancellation.
    /// </summary>
    [DiscordId]
    [StringLength(32)]
    public string ExecutingUserId { get; set; } = null!;

    public bool IsAdminExecution { get; set; }

    public bool NotifyUser { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(RemindId), RemindId.ToString() },
            { nameof(ExecutingUserId), ExecutingUserId },
            { nameof(IsAdminExecution), IsAdminExecution.ToString() },
            { nameof(NotifyUser), NotifyUser.ToString() }
        };
    }
}
