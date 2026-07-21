using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Core.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.GenericParameter, AllowMultiple = false)]
public class RequireSomeItemInCollectionAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null || value is string)
            return ValidationResult.Success;

        if (value is not IEnumerable enumerable)
            return new ValidationResult("Unsupported type of parameter. Only collections (such as arrays, lists, ...) are supported.");

        var enumerator = enumerable.GetEnumerator();
        if (!enumerator.MoveNext())
            return new ValidationResult(ErrorMessage, [validationContext.MemberName!]);
        return ValidationResult.Success;
    }
}
