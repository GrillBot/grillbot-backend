using System.ComponentModel.DataAnnotations;

namespace GrillBot.Core.Validation;

/// <summary>
/// Non-generic face of <see cref="ModelValidator{TModel}"/>, so a caller holding only the
/// runtime model type can run the validator without reflecting over its methods.
/// </summary>
public interface IModelValidator
{
    IEnumerable<ValidationResult> Validate(object model, ValidationContext context);
}
