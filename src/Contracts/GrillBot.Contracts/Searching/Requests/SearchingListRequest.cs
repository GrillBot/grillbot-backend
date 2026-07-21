using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.Searching.Requests;

public class SearchingListRequest : IDictionaryObject
{
    [DiscordId]
    [StringLength(30)]
    public string? UserId { get; set; }

    [DiscordId]
    [StringLength(30)]
    public string? GuildId { get; set; }

    [DiscordId]
    [StringLength(30)]
    public string? ChannelId { get; set; }

    public string? MessageQuery { get; set; }

    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool HideInvalid { get; set; }
    public bool ShowDeleted { get; set; }

    /// <summary>
    /// Available: Id, CreatedAt, ValidTo
    /// Default: Id
    /// </summary>
    public SortParameters Sort { get; set; } = new() { OrderBy = "Id" };

    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(UserId), UserId },
            { nameof(GuildId), GuildId },
            { nameof(ChannelId), ChannelId },
            { nameof(MessageQuery), MessageQuery },
            { nameof(CreatedFrom), CreatedFrom?.ToString("o") },
            { nameof(CreatedTo), CreatedTo?.ToString("o") },
            { nameof(ValidFrom), ValidFrom?.ToString("o") },
            { nameof(ValidTo), ValidTo?.ToString("o") },
            { nameof(HideInvalid), HideInvalid.ToString() },
            { nameof(ShowDeleted), ShowDeleted.ToString() }
        };

        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));

        return result;
    }
}
