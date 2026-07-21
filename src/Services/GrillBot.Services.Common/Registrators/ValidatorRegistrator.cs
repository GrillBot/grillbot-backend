using GrillBot.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GrillBot.Services.Common.Registrators;

public static class ValidatorRegistrator
{
    /// <summary>
    /// Registers every ModelValidator&lt;TModel&gt; in the assembly under its closed generic base
    /// type, which is how <see cref="Infrastructure.Api.Filters.ModelValidationFilter"/> looks
    /// a validator up from the runtime type of a bound action argument.
    /// </summary>
    public static void RegisterValidatorsFromAssembly(this IServiceCollection services, Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        foreach (var type in assembly.GetTypes().Where(o => o.IsClass && !o.IsAbstract))
        {
            var validatorBase = FindValidatorBase(type);
            if (validatorBase is not null)
                services.Add(ServiceDescriptor.Describe(validatorBase, type, lifetime));
        }
    }

    private static Type? FindValidatorBase(Type type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(ModelValidator<>))
                return current;
        }

        return null;
    }
}
