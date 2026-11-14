using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace GrillBot.Core.Services.IdentityService;

public static class IdentityServiceExtensions
{
    /// <summary>
    /// Registers <see cref="IIdentityTokenClient"/> configured from <c>Services:IdentityService</c>.
    /// The section is validated on startup, so a host without the client secret fails fast.
    /// </summary>
    public static IServiceCollection AddIdentityTokenClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddOptions<IdentityClientOptions>()
            .Bind(configuration.GetSection(IdentityClientOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IIdentityTokenClient, IdentityTokenClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<IdentityClientOptions>>().Value;

            client.BaseAddress = new Uri(options.Api.EndsWith('/') ? options.Api : options.Api + "/");
            client.Timeout = options.Timeout;
        });

        return services;
    }
}
