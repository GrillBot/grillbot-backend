using System.ComponentModel.DataAnnotations;

namespace GrillBot.Core.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class RequireTimezoneAttribute : ValidationAttribute
{
    public RequireTimezoneAttribute()
    {
        ErrorMessage = "The DateTime type require a time zone.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
                return new ValidationResult(ErrorMessage, [validationContext.MemberName!]);

            return ValidationResult.Success;
        }

        return new ValidationResult("Object is not DateTime object.", [validationContext.MemberName!]);
    }
}
