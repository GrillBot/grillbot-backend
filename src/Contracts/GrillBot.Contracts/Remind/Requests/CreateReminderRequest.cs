using Discord;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.Remind.Requests;

public class CreateReminderRequest : IDictionaryObject
{
    [DiscordId]
    [StringLength(32)]
    public string FromUserId { get; set; } = null!;

    [DiscordId]
    [StringLength(32)]
    public string ToUserId { get; set; } = null!;

    public DateTime NotifyAtUtc { get; set; }

    [Required(ErrorMessage = "RemindModule/Create/Validation/MessageRequired")]
    [StringLength(EmbedFieldBuilder.MaxFieldValueLength, ErrorMessage = "RemindModule/Create/Validation/MaxLengthExceeded")]
    public string Message { get; set; } = null!;

    [DiscordId]
    [StringLength(32)]
    public string CommandMessageId { get; set; } = null!;

    [StringLength(32)]
    public string Language { get; set; } = null!;

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(FromUserId), FromUserId },
            { nameof(ToUserId), ToUserId },
            { nameof(NotifyAtUtc), NotifyAtUtc.ToString("o") },
            { nameof(Message), Message },
            { nameof(CommandMessageId), CommandMessageId },
            { nameof(Language), Language }
        };
    }
}
