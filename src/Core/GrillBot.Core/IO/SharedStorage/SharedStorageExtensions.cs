using GrillBot.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GrillBot.Core.IO.SharedStorage;

public static class SharedStorageExtensions
{
    /// <summary>
    /// Registers <see cref="SharedFileStorage"/> over the <c>SharedStorage</c> section. The host does not start
    /// when the path is missing, relative or not writable (for example a missing volume mount).
    /// </summary>
    public static IServiceCollection AddSharedFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<SharedStorageOptions, SharedStorageOptionsValidator>(configuration, SharedStorageOptions.SectionName);
        services.AddSingleton(provider => new SharedFileStorage(provider.GetRequiredService<IOptions<SharedStorageOptions>>()));

        return services;
    }
}
