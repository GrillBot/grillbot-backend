using GrillBot.Core.Infrastructure;

namespace PointsService.Models;

public class TransferPointsRequest : IDictionaryObject
{
    public string GuildId { get; set; } = null!;
    public string FromUserId { get; set; } = null!;
    public string ToUserId { get; set; } = null!;
    public int Amount { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId },
            { nameof(FromUserId), FromUserId },
            { nameof(ToUserId), ToUserId },
            { nameof(Amount), Amount.ToString() }
        };
    }
}
