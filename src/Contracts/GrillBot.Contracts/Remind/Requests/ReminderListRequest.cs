using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.Remind.Requests;

public class ReminderListRequest : IDictionaryObject
{
    [DiscordId]
    [StringLength(32)]
    public string? FromUserId { get; set; }

    [DiscordId]
    [StringLength(32)]
    public string? ToUserId { get; set; }

    [DiscordId]
    [StringLength(32)]
    public string? CommandMessageId { get; set; }

    public string? MessageContains { get; set; }

    public DateTime? NotifyAtFromUtc { get; set; }
    public DateTime? NotifyAtToUtc { get; set; }

    public bool? OnlyPending { get; set; }
    public bool? OnlyInProcess { get; set; }

    public SortParameters Sort { get; set; } = new() { OrderBy = "Id" };
    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(FromUserId), FromUserId },
            { nameof(ToUserId), ToUserId },
            { nameof(CommandMessageId), CommandMessageId },
            { nameof(MessageContains), MessageContains },
            { nameof(NotifyAtFromUtc), NotifyAtFromUtc?.ToString("o") },
            { nameof(NotifyAtToUtc), NotifyAtToUtc?.ToString("o") },
            { nameof(OnlyPending), OnlyPending?.ToString() },
            { nameof(OnlyInProcess), OnlyInProcess?.ToString() }
        };

        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        return result;
    }
}
