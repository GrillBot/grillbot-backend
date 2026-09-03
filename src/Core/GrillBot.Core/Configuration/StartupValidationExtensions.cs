using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace GrillBot.Core.Configuration;

/// <summary>
/// Startup validation for the configuration a host cannot run without.
///
/// Every <c>appsettings.json</c> in this repository declares its sensitive keys as empty
/// placeholders and expects the real values to arrive from <c>/run/secrets</c>, an
/// <c>appsettings.{Environment}.json</c> or the environment. A missing override therefore
/// does not look missing - it looks like an empty string. Without validation the host
/// starts, reports healthy, and only fails much later on the first operation that needs
/// the value: a Discord login that never happens, an EF Core connection with an empty
/// connection string, a Wolverine listener bound to the wrong broker.
///
/// The helpers below turn that into a startup failure instead. Options are bound, checked
/// against their DataAnnotations plus any registered <see cref="IValidateOptions{TOptions}"/>,
/// and the check runs while the host is starting rather than on first resolve.
/// <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/> rejects an empty
/// string by default, so an unfilled placeholder is exactly what fails.
/// </summary>
public static class StartupValidationExtensions
{
    /// <summary>
    /// Binds <typeparamref name="TOptions"/> to <paramref name="sectionName"/> and validates it
    /// while the host starts.
    /// </summary>
    public static OptionsBuilder<TOptions> AddValidatedOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName
    ) where TOptions : class
    {
        return services
            .AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    /// <summary>
    /// The same, plus a custom validator for the rules that DataAnnotations cannot express.
    /// The validator is registered once per options type, so calling this repeatedly is safe.
    /// </summary>
    public static OptionsBuilder<TOptions> AddValidatedOptions<TOptions, TValidator>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName
    )
        where TOptions : class
        where TValidator : class, IValidateOptions<TOptions>
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<TOptions>, TValidator>());
        return services.AddValidatedOptions<TOptions>(configuration, sectionName);
    }

    /// <summary>
    /// Validates the section only when the application declares it.
    ///
    /// A declared placeholder is what makes a value mandatory here: a host that needs a
    /// database or a Redis server ships the (empty) section in its <c>appsettings.json</c>,
    /// and one that does not - ImageProcessingService has no <c>ConnectionStrings</c>, most
    /// services have no <c>Redis</c> - ships nothing and is left alone. That keeps the rule
    /// in the application's own configuration file instead of in a list maintained here.
    /// </summary>
    /// <returns>
    /// The options builder, or <see langword="null"/> when the section is absent and nothing
    /// was registered.
    /// </returns>
    public static OptionsBuilder<TOptions>? AddValidatedOptionsWhenDeclared<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName
    ) where TOptions : class
    {
        return configuration.GetSection(sectionName).Exists()
            ? services.AddValidatedOptions<TOptions>(configuration, sectionName)
            : null;
    }
}
