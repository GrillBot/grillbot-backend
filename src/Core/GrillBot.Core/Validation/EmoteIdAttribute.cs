using System.ComponentModel.DataAnnotations;
using Discord;

namespace GrillBot.Core.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class EmoteIdAttribute : ValidationAttribute
{
    public EmoteIdAttribute()
    {
        ErrorMessage = "Emote ID is in an invalid format.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string val)
            return ValidationResult.Success;

        return !Emote.TryParse(val, out _) ? new ValidationResult(ErrorMessage, [validationContext.MemberName!]) : ValidationResult.Success;
    }
}
