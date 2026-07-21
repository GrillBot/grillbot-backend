using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace Emote.Models.Request;

public class EmoteStatisticsListRequest : IDictionaryObject
{
    public bool Unsupported { get; set; }

    [DiscordId]
    [StringLength(32)]
    public string? GuildId { get; set; }

    [DiscordId]
    [StringLength(32)]
    public string? UserId { get; set; }

    public int? UseCountFrom { get; set; }
    public int? UseCountTo { get; set; }
    public DateTime? FirstOccurenceFrom { get; set; }
    public DateTime? FirstOccurenceTo { get; set; }
    public DateTime? LastOccurenceFrom { get; set; }
    public DateTime? LastOccurenceTo { get; set; }

    public bool IgnoreAnimated { get; set; }

    [StringLength(128)]
    public string? EmoteName { get; set; }

    [StringLength(255)]
    public string? EmoteFullId { get; set; }

    public SortParameters Sort { get; set; } = new() { Descending = true, OrderBy = "UseCount" };
    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(Unsupported), Unsupported.ToString() },
            { nameof(GuildId), GuildId },
            { nameof(UserId), UserId },
            { nameof(UseCountFrom), UseCountFrom?.ToString() },
            { nameof(UseCountTo), UseCountTo?.ToString() },
            { nameof(FirstOccurenceFrom), FirstOccurenceFrom?.ToString("o") },
            { nameof(FirstOccurenceTo), FirstOccurenceTo?.ToString("o") },
            { nameof(LastOccurenceFrom), LastOccurenceFrom?.ToString("o") },
            { nameof(LastOccurenceTo), LastOccurenceTo?.ToString("o") },
            { nameof(IgnoreAnimated), IgnoreAnimated.ToString() },
            { nameof(EmoteName), EmoteName },
            { nameof(EmoteFullId), EmoteFullId }
        };

        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));

        return result;
    }
}
