using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.ImageProcessing;

public class AvatarInfo : IValidatableObject
{
    [Required]
    public string AvatarId { get; set; } = null!;

    [Required]
    public byte[] AvatarContent { get; set; } = null!;

    [Required]
    public string Type { get; set; } = null!;

    // Kept on the contract rather than moved to a ModelValidator: AvatarInfo is only ever
    // reached as a nested property of another request, and the validation filter resolves
    // validators for top-level action arguments only. The rule is also self-contained - it
    // needs nothing from the service.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Type != "gif" && Type != "png")
            yield return new ValidationResult("Only gifs and png avatars are allowed.", [nameof(Type)]);
    }
}
