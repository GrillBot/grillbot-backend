using System.ComponentModel.DataAnnotations;
using GrillBot.Core.Infrastructure;

namespace GrillBot.Core.Models.Pagination;

public class PaginatedParams : IDictionaryObject
{
    [Range(0, int.MaxValue)]
    public int Page { get; set; }

    [Range(0, int.MaxValue)]
    public int PageSize { get; set; } = 25;

    public bool OnlyCount { get; set; }

    public int Skip()
        => Math.Max(Page, 0) * PageSize;

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(Page), Page.ToString() },
            { nameof(PageSize), PageSize.ToString() },
            { nameof(OnlyCount), OnlyCount.ToString() }
        };
    }
}
