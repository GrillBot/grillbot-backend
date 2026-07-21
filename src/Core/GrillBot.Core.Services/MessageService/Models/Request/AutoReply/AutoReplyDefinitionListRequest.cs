using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models;
using GrillBot.Core.Models.Pagination;

namespace MessageService.Models.Request.AutoReply;

public class AutoReplyDefinitionListRequest : IDictionaryObject
{
    public string? TemplateContains { get; set; }
    public string? ReplyContains { get; set; }
    public bool HideDisabled { get; set; }

    public SortParameters Sort { get; set; } = new() { OrderBy = "Template" };
    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(TemplateContains), TemplateContains },
            { nameof(ReplyContains), ReplyContains },
            { nameof(HideDisabled), HideDisabled.ToString() }
        };

        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));

        return result;
    }
}
