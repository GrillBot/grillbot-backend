using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models;
using GrillBot.Core.Models.Pagination;
using UnverifyService.Core.Enums;

namespace UnverifyService.Models.Request.Logs;

public class UnverifyLogListRequest : IDictionaryObject
{
    public UnverifyOperationType? Operation { get; set; }
    public string? FromUserId { get; set; }
    public string? ToUserId { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public string? GuildId { get; set; }
    public Guid? ParentLogItemId { get; set; }

    /// <summary>
    /// Possible values: Crated, Operation
    /// Default: Created
    /// </summary>
    public SortParameters Sort { get; set; } = new() { OrderBy = "Created" };
    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            ["Operation"] = Operation?.ToString(),
            ["FromUserId"] = FromUserId,
            ["ToUserId"] = ToUserId,
            ["CreatedFrom"] = CreatedFrom?.ToString("o"),
            ["CreatedTo"] = CreatedTo?.ToString("o"),
            ["GuildId"] = GuildId,
            ["ParentLogItemId"] = ParentLogItemId?.ToString()
        };

        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));

        return result;
    }
}
