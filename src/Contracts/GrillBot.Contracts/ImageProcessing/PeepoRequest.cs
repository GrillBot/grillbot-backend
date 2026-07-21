using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.ImageProcessing;

public class PeepoRequest
{
    /// <summary>
    /// Guild upload limit in bytes.
    /// </summary>
    [Required]
    [Range(0, long.MaxValue)]
    public long GuildUploadLimit { get; set; }

    [Required]
    [StringLength(30)]
    [DiscordId]
    public string UserId { get; set; } = null!;

    [Required]
    public AvatarInfo AvatarInfo { get; set; } = null!;
}
