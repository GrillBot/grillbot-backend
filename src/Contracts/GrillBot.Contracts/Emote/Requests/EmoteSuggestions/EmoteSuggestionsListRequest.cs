using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;

namespace GrillBot.Contracts.Emote.Requests.EmoteSuggestions;

public class EmoteSuggestionsListRequest : IDictionaryObject
{
    [DiscordId]
    public string? GuildId { get; set; }

    [DiscordId]
    public string? FromUserId { get; set; }
    public DateTime? SuggestedFrom { get; set; }
    public DateTime? SuggestedTo { get; set; }
    public string? NameContains { get; set; }
    public bool? ApprovalState { get; set; }

    public PaginatedParams Pagination { get; set; } = new();

    /// <summary>
    /// Available: SuggestedAt, Name
    /// Default: SuggestedAt
    /// </summary>
    public SortParameters Sort { get; set; } = new() { OrderBy = "SuggestedAt" };

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId },
            { nameof(FromUserId), FromUserId },
            { nameof(SuggestedFrom), SuggestedFrom?.ToString("o") },
            { nameof(SuggestedTo), SuggestedTo?.ToString("o") },
            { nameof(NameContains), NameContains },
            { nameof(ApprovalState), ApprovalState?.ToString() }
        };

        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        result.MergeDictionaryObjects(Sort, nameof(Sort));

        return result;
    }
}
