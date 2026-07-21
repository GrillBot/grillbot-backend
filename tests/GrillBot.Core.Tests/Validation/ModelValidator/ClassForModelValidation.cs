using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Core.Tests.Validation.ModelValidator;

public class ClassForModelValidation : ModelValidator<DataModelClass>
{
    protected override IEnumerable<Func<DataModelClass, ValidationContext, IEnumerable<ValidationResult>>> GetValidations()
    {
        yield return ValidateDateTime;
        yield return ValidateStringValue;
        yield return ValidateInt;
    }

    private static IEnumerable<ValidationResult> ValidateDateTime(DataModelClass request, ValidationContext _)
    {
        return new List<ValidationResult?>
        {
            CheckUtcDateTime(request.Value2, nameof(request.Value2)),
            CheckUtcDateTime(request.Value3, nameof(request.Value3)),
            CheckDateTimeRange(request.Value2, request.Value3, nameof(request.Value2), nameof(request.Value3))
        }.Where(o => o is not null)!;
    }

    private static IEnumerable<ValidationResult> ValidateStringValue(DataModelClass request, ValidationContext _)
    {
        if (string.IsNullOrEmpty(request.Value1))
            yield return new ValidationResult("String value is required", [nameof(request.Value1)]);
    }

    private static IEnumerable<ValidationResult> ValidateInt(DataModelClass request, ValidationContext _)
    {
        var result = CheckDurationRange(request.Value4, request.Value5, nameof(request.Value4), nameof(request.Value5));
        if (result is not null)
            yield return result;
    }
}
