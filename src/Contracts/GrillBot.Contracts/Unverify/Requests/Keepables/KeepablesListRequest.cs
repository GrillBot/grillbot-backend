using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models;
using GrillBot.Core.Models.Pagination;

namespace GrillBot.Contracts.Unverify.Requests.Keepables;

public class KeepablesListRequest : IDictionaryObject
{
    public string? Group { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    /// <summary>
    /// Possible values: Group, Name, Created
    /// Default: Group
    /// </summary>
    public SortParameters Sort { get; set; } = new() { OrderBy = "Group" };
    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            ["Group"] = Group,
            ["CreatedFrom"] = CreatedFrom?.ToString("o"),
            ["CreatedTo"] = CreatedTo?.ToString("o"),
        };

        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));

        return result;
    }
}
