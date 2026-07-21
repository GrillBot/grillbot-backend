using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models;
using GrillBot.Core.Models.Pagination;

namespace UnverifyService.Models.Request;

public class ActiveUnverifyListRequest : IDictionaryObject
{
    public string? GuildId { get; set; }

    /// <summary>
    /// Default: StartAt,
    /// PossibleValues: StartAt, EndAt
    /// </summary>
    public SortParameters Sort { get; set; } = new() { OrderBy = "StartAt" };
    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            ["GuildId"] = GuildId
        };

        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));

        return result;
    }
}
