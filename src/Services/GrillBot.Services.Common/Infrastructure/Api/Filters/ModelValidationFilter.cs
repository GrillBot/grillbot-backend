using GrillBot.Core.Validation;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Services.Common.Infrastructure.Api.Filters;

/// <summary>
/// Runs the <see cref="ModelValidator{TModel}"/> registered for each bound action argument.
///
/// Request contracts used to carry their own validation by implementing
/// <see cref="IValidatableObject"/>, which tied the shared contract type to a service-local
/// validator (and its service-local options). The contracts now live in GrillBot.Contracts
/// and know nothing about validation; the owning service registers a validator instead and
/// this filter connects the two.
///
/// Order is below the -2000 used by the [ApiController] model-state filter, so failures added
/// here still turn into the same automatic 400 ValidationProblemDetails response as before.
/// </summary>
public class ModelValidationFilter : IAsyncActionFilter, IOrderedFilter
{
    public int Order => -3000;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validator = FindValidator(services, argument.GetType());
            if (validator is null)
                continue;

            foreach (var result in validator.Validate(argument, new ValidationContext(argument, services, null)))
            {
                if (result.MemberNames.Any())
                {
                    foreach (var memberName in result.MemberNames)
                        context.ModelState.AddModelError(memberName, result.ErrorMessage ?? "");
                }
                else
                {
                    context.ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "");
                }
            }
        }

        await next();
    }

    private static IModelValidator? FindValidator(IServiceProvider services, Type modelType)
    {
        if (!modelType.IsClass)
            return null;

        var validatorType = typeof(ModelValidator<>).MakeGenericType(modelType);
        return services.GetService(validatorType) as IModelValidator;
    }
}
