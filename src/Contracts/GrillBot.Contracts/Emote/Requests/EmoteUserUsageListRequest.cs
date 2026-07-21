using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.Emote.Requests;

public class EmoteUserUsageListRequest : IDictionaryObject
{
    [DiscordId]
    [StringLength(32)]
    public string GuildId { get; set; } = null!;

    [EmoteId]
    [StringLength(255)]
    public string EmoteId { get; set; } = null!;

    public SortParameters Sort { get; set; } = new() { Descending = true, OrderBy = "UseCount" };
    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId },
            { nameof(EmoteId), EmoteId }
        };

        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));

        return result;
    }
}
