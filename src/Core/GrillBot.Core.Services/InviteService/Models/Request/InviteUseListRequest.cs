using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace InviteService.Models.Request;

public class InviteUseListRequest : IDictionaryObject
{
    [DiscordId]
    [StringLength(32)]
    public string GuildId { get; set; } = null!;

    [StringLength(10)]
    public string Code { get; set; } = null!;

    public DateTime? UsedFrom { get; set; }
    public DateTime? UsedTo { get; set; }

    /// <summary>
    /// Available: UsedAt,
    /// Default: UsedAt
    /// </summary>
    public SortParameters Sort { get; set; } = new() { OrderBy = "UsedAt" };

    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId },
            { nameof(Code), Code },
            { nameof(UsedFrom), UsedFrom?.ToString("o") },
            { nameof(UsedTo), UsedTo?.ToString("o") },
        };

        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));

        return result;
    }
}
