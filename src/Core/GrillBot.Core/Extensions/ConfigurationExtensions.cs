using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.KeyPerFile;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace GrillBot.Core;

/// <summary>
/// Configuration wiring shared by every runnable host (the bot and all services).
///
/// Non-sensitive configuration (URLs, logging levels, feature toggles) lives in
/// <c>appsettings.json</c> and environment variables. Sensitive values are supplied
/// out-of-band: in production from Docker (Swarm) secrets mounted at
/// <c>/run/secrets</c>, and in development from a git-ignored
/// <c>appsettings.Development.json</c> (loaded automatically by the default host builder).
/// </summary>
public static class ConfigurationExtensions
{
    private const string DockerSecretsPath = "/run/secrets";

    /// <summary>
    /// Optionally loads Docker/Swarm secrets from <c>/run/secrets</c>. Does nothing when the
    /// directory is absent (e.g. local runs), so it is safe to call unconditionally.
    ///
    /// Two secret file layouts are supported:
    /// <list type="bullet">
    /// <item>Key-per-file — the file name is the configuration key, the raw file content is its value.</item>
    /// <item>JSON files (<c>*.json</c>) — appsettings-shaped documents merged over existing configuration.</item>
    /// </list>
    ///
    /// The loader is intentionally generic: it makes no assumption about which keys are present.
    /// Precedence (lowest to highest): appsettings.json &lt; appsettings.{Environment}.json &lt;
    /// /run/secrets &lt; environment variables (re-applied last so they always win).
    /// </summary>
    public static IConfigurationBuilder AddDockerSecrets(this IConfigurationBuilder builder)
    {
        if (!Directory.Exists(DockerSecretsPath))
            return builder;

        // Key-per-file secrets. Skip dotfiles and *.json (JSON secrets are handled below so
        // they aren't collapsed into a single opaque string value).
        builder.AddKeyPerFile(source =>
        {
            source.FileProvider = new PhysicalFileProvider(DockerSecretsPath);
            source.Optional = true;
            source.ReloadOnChange = false;
            source.IgnoreCondition = fileName =>
                fileName.StartsWith('.') ||
                Path.GetExtension(fileName).Equals(".json", StringComparison.OrdinalIgnoreCase);
        });

        // JSON secrets. Loaded blindly (by full path, not name-filtered) so any keys inside
        // override previously-loaded configuration.
        foreach (var file in Directory.GetFiles(DockerSecretsPath, "*.json", SearchOption.TopDirectoryOnly))
            builder.AddJsonFile(file, optional: true, reloadOnChange: false);

        // Re-append environment variables so they keep the highest precedence.
        ReCreateEnvironmentVariables(builder);
        return builder;
    }

    /// <summary>
    /// Logs which configuration providers were loaded and, for file-based ones, from where.
    /// Useful for verifying at startup that Docker secrets were picked up in production.
    /// </summary>
    public static void LogConfigurationSources(this IConfiguration configuration, ILogger logger)
    {
        if (configuration is not IConfigurationRoot root)
            return;

        logger.LogInformation("=== Loaded configuration sources ===");

        foreach (var provider in root.Providers)
        {
            switch (provider)
            {
                case JsonConfigurationProvider json:
                    logger.LogInformation("Configuration source: JSON file -> {Path} (optional: {Optional})", json.Source.Path, json.Source.Optional);
                    break;
                case KeyPerFileConfigurationProvider keyPerFile:
                    logger.LogInformation("Configuration source: {Provider}", keyPerFile.ToString());
                    break;
                default:
                    logger.LogInformation("Configuration source: {Provider}", provider.GetType().Name);
                    break;
            }
        }
    }

    private static void ReCreateEnvironmentVariables(IConfigurationBuilder builder)
    {
        var environmentSources = builder.Sources
            .OfType<EnvironmentVariablesConfigurationSource>()
            .ToList();

        foreach (var source in environmentSources)
        {
            builder.Sources.Remove(source);
            builder.AddEnvironmentVariables(source.Prefix);
        }
    }
}
