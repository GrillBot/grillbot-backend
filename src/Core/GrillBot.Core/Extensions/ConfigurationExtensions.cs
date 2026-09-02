using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
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
    /// The secret sources are inserted directly below the host's environment-variable source
    /// rather than appended, which keeps the default ASP.NET Core precedence intact and adds
    /// secrets to it in one place. Precedence (lowest to highest): appsettings.json &lt;
    /// appsettings.{Environment}.json &lt; user secrets &lt; /run/secrets &lt; environment
    /// variables &lt; command-line arguments.
    /// </summary>
    public static IConfigurationBuilder AddDockerSecrets(this IConfigurationBuilder builder)
        => builder.AddDockerSecrets(DockerSecretsPath);

    /// <inheritdoc cref="AddDockerSecrets(IConfigurationBuilder)"/>
    /// <param name="builder">The configuration builder to add the secret sources to.</param>
    /// <param name="secretsPath">
    /// Directory the secrets are mounted in. Production always uses <c>/run/secrets</c>; this
    /// overload exists so the behaviour can be exercised against a temporary directory.
    /// </param>
    public static IConfigurationBuilder AddDockerSecrets(this IConfigurationBuilder builder, string secretsPath)
    {
        if (!Directory.Exists(secretsPath))
            return builder;

        var fileProvider = new PhysicalFileProvider(Path.GetFullPath(secretsPath));
        var sources = new List<IConfigurationSource>
        {
            // Key-per-file secrets. Skip dotfiles and *.json (JSON secrets are handled below so
            // they aren't collapsed into a single opaque string value).
            new KeyPerFileConfigurationSource
            {
                FileProvider = fileProvider,
                Optional = true,
                ReloadOnChange = false,
                IgnoreCondition = fileName =>
                    fileName.StartsWith('.') ||
                    Path.GetExtension(fileName).Equals(".json", StringComparison.OrdinalIgnoreCase)
            }
        };

        // JSON secrets. Loaded blindly (every *.json in the directory, not name-filtered) so any
        // keys inside override previously-loaded configuration. Ordered so the merge result does
        // not depend on the order the filesystem happens to return.
        var jsonFiles = Directory.GetFiles(secretsPath, "*.json", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .OfType<string>()
            .Order(StringComparer.Ordinal);

        foreach (var fileName in jsonFiles)
        {
            sources.Add(new JsonConfigurationSource
            {
                FileProvider = fileProvider,
                Path = fileName,
                Optional = true,
                ReloadOnChange = false
            });
        }

        var index = FindSecretsInsertIndex(builder.Sources);
        foreach (var source in sources)
            builder.Sources.Insert(index++, source);

        return builder;
    }

    /// <summary>
    /// Logs which configuration providers were loaded and, for file-based ones, from where.
    /// Useful for verifying at startup that Docker secrets were picked up in production, and
    /// that the environment-variable provider is present at the expected precedence.
    /// </summary>
    public static void LogConfigurationSources(this IConfiguration configuration, ILogger logger)
    {
        if (configuration is not IConfigurationRoot root)
            return;

        logger.LogInformation("=== Loaded configuration sources (lowest precedence first) ===");

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
                case EnvironmentVariablesConfigurationProvider environmentVariables:
                    logger.LogInformation("Configuration source: {Provider}", environmentVariables.ToString());
                    break;
                default:
                    logger.LogInformation("Configuration source: {Provider}", provider.GetType().Name);
                    break;
            }
        }
    }

    /// <summary>
    /// Finds the position secrets have to take to sit above the appsettings files but below the
    /// environment variables and the command line. That is the index of the first unprefixed
    /// environment-variable source — the one the host adds for application configuration; the
    /// prefixed <c>DOTNET_</c>/<c>ASPNETCORE_</c> sources carry host configuration and are
    /// deliberately skipped. Falls back to the command line, then to appending.
    /// </summary>
    private static int FindSecretsInsertIndex(IList<IConfigurationSource> sources)
    {
        var index = IndexOf(sources, source => source is EnvironmentVariablesConfigurationSource { Prefix: null or "" });
        if (index >= 0)
            return index;

        index = IndexOf(sources, source => source is CommandLineConfigurationSource);
        return index >= 0 ? index : sources.Count;
    }

    private static int IndexOf(IList<IConfigurationSource> sources, Func<IConfigurationSource, bool> predicate)
    {
        for (var i = 0; i < sources.Count; i++)
        {
            if (predicate(sources[i]))
                return i;
        }

        return -1;
    }
}
