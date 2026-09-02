using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.KeyPerFile;
using Microsoft.Extensions.Configuration.Memory;

namespace GrillBot.Core.Tests.Extensions;

/// <summary>
/// Covers <see cref="ConfigurationExtensions.AddDockerSecrets(IConfigurationBuilder, string)"/>,
/// and in particular where the secret sources land: they have to override the appsettings files
/// while still losing to the environment variables and the command line, which is what makes an
/// operator able to override a stale secret without redeploying.
/// </summary>
[TestClass]
[DoNotParallelize]
public class ConfigurationExtensionsTests
{
    /// <summary>
    /// Written as a Docker secret file name. The environment-variable provider normalizes the
    /// double underscore to the <c>:</c> key delimiter, so both spellings address
    /// <c>ConnectionStrings:Default</c>.
    /// </summary>
    private const string ConnectionStringVariable = "ConnectionStrings__Default";

    private string SecretsPath { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        SecretsPath = Path.Combine(Path.GetTempPath(), $"grillbot-secrets-{Guid.NewGuid():N}");
        Directory.CreateDirectory(SecretsPath);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Environment.SetEnvironmentVariable(ConnectionStringVariable, null);

        if (Directory.Exists(SecretsPath))
            Directory.Delete(SecretsPath, true);
    }

    private void WriteSecret(string fileName, string content)
        => File.WriteAllText(Path.Combine(SecretsPath, fileName), content);

    private static int IndexOf(IConfigurationBuilder builder, Func<IConfigurationSource, bool> predicate)
        => builder.Sources.Select((source, index) => (source, index)).First(item => predicate(item.source)).index;

    /// <summary>
    /// Mirrors the source order every host sets up: the appsettings files first, environment
    /// variables above them, the command line highest.
    /// </summary>
    private static ConfigurationBuilder CreateHostLikeBuilder(
        IEnumerable<KeyValuePair<string, string?>>? appSettings = null,
        params string[] commandLineArgs
    )
    {
        var builder = new ConfigurationBuilder();

        builder.AddInMemoryCollection(appSettings ?? []);
        builder.Add(new EnvironmentVariablesConfigurationSource());
        builder.Add(new CommandLineConfigurationSource { Args = commandLineArgs });

        return builder;
    }

    [TestMethod]
    public void AddDockerSecrets_MissingDirectory_IsNoOp()
    {
        var builder = CreateHostLikeBuilder();
        var sourceCount = builder.Sources.Count;

        builder.AddDockerSecrets(Path.Combine(SecretsPath, "does-not-exist"));

        Assert.HasCount(sourceCount, builder.Sources);
    }

    [TestMethod]
    public void AddDockerSecrets_KeyPerFile_IsReadAsConfigurationKey()
    {
        WriteSecret(ConnectionStringVariable, "Host=localhost;Database=GrillBot");

        var configuration = CreateHostLikeBuilder()
            .AddDockerSecrets(SecretsPath)
            .Build();

        Assert.AreEqual("Host=localhost;Database=GrillBot", configuration.GetConnectionString("Default"));
    }

    [TestMethod]
    public void AddDockerSecrets_JsonSecret_IsMergedIntoConfiguration()
    {
        WriteSecret("secrets.json", """{ "RabbitMQ": { "Hostname": "rabbitmq", "Username": "GrillBot" } }""");

        var configuration = CreateHostLikeBuilder()
            .AddDockerSecrets(SecretsPath)
            .Build();

        Assert.AreEqual("rabbitmq", configuration["RabbitMQ:Hostname"]);
        Assert.AreEqual("GrillBot", configuration["RabbitMQ:Username"]);
    }

    [TestMethod]
    public void AddDockerSecrets_JsonSecret_IsNotAlsoReadAsAnOpaqueKeyPerFileValue()
    {
        WriteSecret("secrets.json", """{ "RabbitMQ": { "Hostname": "rabbitmq" } }""");

        var configuration = CreateHostLikeBuilder()
            .AddDockerSecrets(SecretsPath)
            .Build();

        Assert.IsNull(configuration["secrets.json"]);
    }

    [TestMethod]
    public void AddDockerSecrets_DotFiles_AreIgnored()
    {
        WriteSecret(".hidden", "value");

        var configuration = CreateHostLikeBuilder()
            .AddDockerSecrets(SecretsPath)
            .Build();

        Assert.IsNull(configuration[".hidden"]);
    }

    [TestMethod]
    public void AddDockerSecrets_OverridesAppSettings()
    {
        WriteSecret(ConnectionStringVariable, "from-secret");

        var configuration = CreateHostLikeBuilder(
            appSettings: [new("ConnectionStrings:Default", "from-appsettings")]
        ).AddDockerSecrets(SecretsPath).Build();

        Assert.AreEqual("from-secret", configuration.GetConnectionString("Default"));
    }

    [TestMethod]
    public void AddDockerSecrets_LosesToEnvironmentVariables()
    {
        WriteSecret(ConnectionStringVariable, "from-secret");
        Environment.SetEnvironmentVariable(ConnectionStringVariable, "from-environment");

        var configuration = CreateHostLikeBuilder(
            appSettings: [new("ConnectionStrings:Default", "from-appsettings")]
        ).AddDockerSecrets(SecretsPath).Build();

        Assert.AreEqual("from-environment", configuration.GetConnectionString("Default"));
    }

    /// <summary>
    /// Secrets are inserted below the environment-variable source rather than appended, so the
    /// host's own ordering — command line above environment variables — survives untouched.
    /// </summary>
    [TestMethod]
    public void AddDockerSecrets_KeepsCommandLineAsTheHighestPrecedence()
    {
        WriteSecret(ConnectionStringVariable, "from-secret");
        Environment.SetEnvironmentVariable(ConnectionStringVariable, "from-environment");

        var configuration = CreateHostLikeBuilder(
            appSettings: null,
            "--ConnectionStrings:Default=from-command-line"
        ).AddDockerSecrets(SecretsPath).Build();

        Assert.AreEqual("from-command-line", configuration.GetConnectionString("Default"));
    }

    [TestMethod]
    public void AddDockerSecrets_WithoutAnEnvironmentVariableSource_StaysBelowTheCommandLine()
    {
        WriteSecret(ConnectionStringVariable, "from-secret");

        var builder = new ConfigurationBuilder();
        builder.AddInMemoryCollection([]);
        builder.Add(new CommandLineConfigurationSource { Args = ["--ConnectionStrings:Default=from-command-line"] });

        var configuration = builder.AddDockerSecrets(SecretsPath).Build();

        Assert.AreEqual("from-command-line", configuration.GetConnectionString("Default"));
    }

    /// <summary>
    /// The prefixed sources carry host configuration (<c>DOTNET_</c>, <c>ASPNETCORE_</c>) and sit
    /// below the appsettings files, so secrets must not be inserted in front of them.
    /// </summary>
    [TestMethod]
    public void AddDockerSecrets_IsInsertedBelowTheUnprefixedEnvironmentVariableSourceOnly()
    {
        WriteSecret(ConnectionStringVariable, "from-secret");

        var builder = new ConfigurationBuilder();
        builder.Add(new EnvironmentVariablesConfigurationSource { Prefix = "DOTNET_" });
        builder.AddInMemoryCollection([new("ConnectionStrings:Default", "from-appsettings")]);
        builder.Add(new EnvironmentVariablesConfigurationSource());

        builder.AddDockerSecrets(SecretsPath);

        var appSettingsIndex = IndexOf(builder, source => source is MemoryConfigurationSource);
        var secretsIndex = IndexOf(builder, source => source is KeyPerFileConfigurationSource);
        var environmentIndex = IndexOf(builder, source => source is EnvironmentVariablesConfigurationSource { Prefix: null or "" });

        Assert.IsGreaterThan(appSettingsIndex, secretsIndex, "Secrets must override the appsettings files.");
        Assert.IsLessThan(environmentIndex, secretsIndex, "Secrets must lose to the environment variables.");
        Assert.AreEqual("from-secret", builder.Build().GetConnectionString("Default"));
    }
}
