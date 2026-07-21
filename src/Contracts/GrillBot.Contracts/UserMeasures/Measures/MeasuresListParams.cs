using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.UserMeasures.Measures;

public class MeasuresListParams : IDictionaryObject, IValidatableObject
{
    /// <summary>
    /// Possible values: Unverify, Timeout, Warning
    /// </summary>
    [StringLength(10)]
    public string? Type { get; set; }

    [DiscordId]
    public string? GuildId { get; set; }

    [DiscordId]
    public string? UserId { get; set; }

    [DiscordId]
    public string? ModeratorId { get; set; }

    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(Type), Type },
            { nameof(GuildId), GuildId },
            { nameof(UserId), UserId },
            { nameof(ModeratorId), ModeratorId },
            { nameof(CreatedFrom), CreatedFrom?.ToString("o") },
            { nameof(CreatedTo), CreatedTo?.ToString("o") }
        };

        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        return result;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CreatedFrom > CreatedTo)
            yield return new ValidationResult("Invalid interval From-To", [nameof(CreatedFrom), nameof(CreatedTo)]);

        // The service-side copy of this contract enforced the allowed values while the client
        // copy enforced the interval. Neither side saw both rules until now.
        var possibleTypes = new[] { "Unverify", "Timeout", "Warning" };
        if (!string.IsNullOrEmpty(Type) && !possibleTypes.Contains(Type))
            yield return new ValidationResult("Unsupported type.", [nameof(Type)]);
    }
}
