using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models.Pagination;
using System.ComponentModel.DataAnnotations;

namespace PointsService.Models.Users;

public class UserListRequest : IDictionaryObject
{
    [StringLength(30)]
    public string? GuildId { get; set; }

    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId }
        };

        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        return result;
    }
}
