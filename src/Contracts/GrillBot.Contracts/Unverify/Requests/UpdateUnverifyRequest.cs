using GrillBot.Core.Infrastructure;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.Unverify.Requests;

public class UpdateUnverifyRequest : IDictionaryObject
{
    [DiscordId]
    [StringLength(32)]
    public string GuildId { get; set; } = null!;

    [DiscordId]
    [StringLength(32)]
    public string UserId { get; set; } = null!;

    public DateTime NewEndAtUtc { get; set; }
    public string? Reason { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId },
            { nameof(UserId), UserId },
            { nameof(NewEndAtUtc), NewEndAtUtc.ToString("o") },
            { nameof(Reason), Reason }
        };
    }
}
