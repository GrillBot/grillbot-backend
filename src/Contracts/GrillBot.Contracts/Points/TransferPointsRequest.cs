using GrillBot.Core.Infrastructure;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.Points;

public class TransferPointsRequest : IValidatableObject, IDictionaryObject
{
    [Required]
    [StringLength(30)]
    [DiscordId]
    public string GuildId { get; set; } = null!;

    [Required]
    [StringLength(30)]
    [DiscordId]
    public string FromUserId { get; set; } = null!;

    [Required]
    [StringLength(30)]
    [DiscordId]
    public string ToUserId { get; set; } = null!;

    [Required]
    [Range(1, int.MaxValue)]
    public int Amount { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FromUserId == ToUserId)
            yield return new ValidationResult("Unable to transfer points between same accounts.", [nameof(FromUserId), nameof(ToUserId)]);
    }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId },
            { nameof(FromUserId), FromUserId },
            { nameof(ToUserId), ToUserId },
            { nameof(Amount), Amount.ToString() }
        };
    }
}
