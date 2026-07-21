using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.ImageProcessing;

public class PointsRequest
{
    [Required]
    [StringLength(30)]
    [DiscordId]
    public string UserId { get; set; } = null!;

    [Required]
    [StringLength(32)]
    public string Username { get; set; } = null!;

    [Required]
    public int PointsValue { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Position { get; set; }

    [Required]
    public AvatarInfo AvatarInfo { get; set; } = null!;
}
