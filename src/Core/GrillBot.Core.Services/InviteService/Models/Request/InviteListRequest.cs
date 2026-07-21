using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace InviteService.Models.Request;

public class InviteListRequest : IValidatableObject, IDictionaryObject
{
    [DiscordId]
    [StringLength(32)]
    public string? GuildId { get; set; }

    [DiscordId]
    [StringLength(32)]
    public string? CreatorId { get; set; }

    public bool OnlyWithoutCreator { get; set; }

    public string? Code { get; set; }

    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    /// <summary>
    /// Available: Code, Created, Uses
    /// Default: Code
    /// </summary>
    public SortParameters Sort { get; set; } = new() { OrderBy = "Code" };

    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId },
            { nameof(CreatorId), CreatorId },
            { nameof(OnlyWithoutCreator), OnlyWithoutCreator.ToString() },
            { nameof(Code), Code },
            { nameof(CreatedFrom), CreatedFrom?.ToString("o") },
            { nameof(CreatedTo), CreatedTo?.ToString("o") }
        };

        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));

        return result;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(CreatorId) && OnlyWithoutCreator)
            yield return new("Cannot combine CreatorId and OnlyWithoutCreator properties.", [nameof(CreatorId), nameof(OnlyWithoutCreator)]);
    }
}
