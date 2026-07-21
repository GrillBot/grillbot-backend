using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Core.Extensions;

public static class ValidationProblemDetailsExtensions
{
    public static void AggregateAndThrow(this ValidationProblemDetails? details)
    {
        if (details is null || details.Errors.Count == 0)
            return;

        var errorsQuery = details.Errors
            .Select(o => $"{o.Key}: {string.Join("; ", o.Value)}")
            .Distinct();
        throw new ValidationException(string.Join("\n", errorsQuery));
    }

    public static void ThrowFirstError(this ValidationProblemDetails? details)
    {
        if (details is null || details.Errors.Count == 0)
            return;

        var error = details.Errors.First();
        throw new ValidationException($"{error.Key}: {string.Join("; ", error.Value)}");
    }
}
