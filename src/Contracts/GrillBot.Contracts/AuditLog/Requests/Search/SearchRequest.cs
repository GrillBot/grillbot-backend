using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models;
using GrillBot.Core.Models.Pagination;
using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.AuditLog.Requests.Search;

public class SearchRequest : IDictionaryObject
{
    [DiscordId]
    [StringLength(32)]
    public string? GuildId { get; set; }

    [DiscordId]
    public List<string> UserIds { get; set; } = [];

    [DiscordId]
    [StringLength(32)]
    public string? ChannelId { get; set; }
    public List<LogType> ShowTypes { get; set; } = [];
    public List<LogType> IgnoreTypes { get; set; } = [];
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public bool OnlyWithFiles { get; set; }
    public List<Guid>? Ids { get; set; }
    public AdvancedSearchRequest? AdvancedSearch { get; set; }
    public SortParameters Sort { get; set; } = new() { Descending = true, OrderBy = "CreatedAt" };
    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId },
            { nameof(CreatedFrom), CreatedFrom?.ToString("o") },
            { nameof(CreatedTo), CreatedTo?.ToString("o") },
            { nameof(OnlyWithFiles), OnlyWithFiles.ToString() },
            { nameof(ChannelId), ChannelId }
        };

        for (var i = 0; i < UserIds.Count; i++)
            result.Add($"{nameof(UserIds)}[{i}]", UserIds[i]);
        for (var i = 0; i < ShowTypes.Count; i++)
            result.Add($"{nameof(ShowTypes)}[{i}]", ShowTypes[i].ToString());
        for (var i = 0; i < IgnoreTypes.Count; i++)
            result.Add($"{nameof(IgnoreTypes)}[{i}]", IgnoreTypes[i].ToString());

        if (Ids is not null)
        {
            for (var i = 0; i < Ids.Count; i++)
                result.Add($"{nameof(Ids)}[{i}]", Ids[i].ToString());
        }

        result.MergeDictionaryObjects(AdvancedSearch, nameof(AdvancedSearch));
        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        return result;
    }
}
