using System.ComponentModel.DataAnnotations;

namespace GrillBot.Core.Validation;

public abstract class ModelValidator<TModel> where TModel : class
{
    public IEnumerable<ValidationResult> Validate(TModel model, ValidationContext context)
        => GetValidations().SelectMany(method => method(model, context));

    protected abstract IEnumerable<Func<TModel, ValidationContext, IEnumerable<ValidationResult>>> GetValidations();

    protected static ValidationResult? CheckUtcDateTime(DateTime? dateTime, string propertyName)
    {
        if (dateTime.HasValue && dateTime.Value.Kind != DateTimeKind.Utc)
            return new ValidationResult("Only UTC value is allowed.", [propertyName]);
        return null;
    }

    protected static ValidationResult? CheckDurationRange(int? from, int? to, string fromPropertyName, string toPropertyName)
    {
        if (from is null || to is null)
            return null;

        return from > to ? new ValidationResult("Unallowed interval of duration range.", [fromPropertyName, toPropertyName]) : null;
    }

    protected static ValidationResult? CheckDateTimeRange(DateTime? from, DateTime? to, string fromPropertyName, string toPropertyName)
    {
        if (from is null || to is null)
            return null;

        return from > to ? new ValidationResult("Unallowed interval of datetime duration range.", [fromPropertyName, toPropertyName]) : null;
    }
}
