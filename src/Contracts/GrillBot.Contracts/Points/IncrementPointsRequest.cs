using GrillBot.Core.Infrastructure;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.Points;

public class IncrementPointsRequest : IDictionaryObject
{
    [StringLength(30)]
    [DiscordId]
    public string GuildId { get; set; } = null!;

    [StringLength(30)]
    [DiscordId]
    public string UserId { get; set; } = null!;

    public int Amount { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId },
            { nameof(UserId), UserId },
            { nameof(Amount), Amount.ToString() }
        };
    }
}
