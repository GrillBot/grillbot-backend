using GrillBot.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GrillBot.Core.Tests.Configuration;

[TestClass]
public class StartupValidationExtensionsTests
{
    /// <summary>
    /// Runs exactly what the host runs while it starts. Anything registered with
    /// ValidateOnStart() is checked here and nowhere else, so a rule that only fires on first
    /// resolve would not show up in these tests either.
    /// </summary>
    private static void ValidateOnStart(Action<IServiceCollection, IConfiguration> register, params (string Key, string? Value)[] configuration)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configuration.Select(o => new KeyValuePair<string, string?>(o.Key, o.Value)))
            .Build();

        var services = new ServiceCollection();
        register(services, config);

        using var provider = services.BuildServiceProvider();

        // Absent when nothing asked to be validated on start - which is itself the expected
        // outcome for a host that does not declare the section.
        provider.GetService<IStartupValidator>()?.Validate();
    }

    private static void ValidateRabbitMq(params (string Key, string? Value)[] configuration)
        => ValidateOnStart((services, config) => services.AddValidatedOptions<RabbitMQOptions>(config, RabbitMQOptions.SectionName), configuration);

    private static void ValidateConnectionStringsWhenDeclared(params (string Key, string? Value)[] configuration)
        => ValidateOnStart(
            (services, config) => services.AddValidatedOptionsWhenDeclared<ConnectionStringsOptions>(config, ConnectionStringsOptions.SectionName),
            configuration
        );

    [TestMethod]
    public void AddValidatedOptions_FilledSection_Passes()
    {
        ValidateRabbitMq(
            ("RabbitMQ:Hostname", "rabbitmq"),
            ("RabbitMQ:Username", "grillbot"),
            ("RabbitMQ:Password", "secret")
        );
    }

    [TestMethod]
    public void AddValidatedOptions_EmptyPlaceholder_Fails()
    {
        // The exact shape of a committed appsettings.json whose secrets were never overridden.
        var exception = Assert.ThrowsExactly<OptionsValidationException>(() => ValidateRabbitMq(
            ("RabbitMQ:Hostname", ""),
            ("RabbitMQ:Username", ""),
            ("RabbitMQ:Password", "")
        ));

        StringAssert.Contains(string.Join(' ', exception.Failures), "RabbitMQ:Hostname");
    }

    [TestMethod]
    public void AddValidatedOptions_MissingSection_Fails()
    {
        Assert.ThrowsExactly<OptionsValidationException>(() => ValidateRabbitMq());
    }

    [TestMethod]
    public void AddValidatedOptions_ReportsEveryMissingKeyAtOnce()
    {
        var exception = Assert.ThrowsExactly<OptionsValidationException>(() => ValidateRabbitMq(("RabbitMQ:Hostname", "rabbitmq")));

        var failures = string.Join(' ', exception.Failures);
        StringAssert.Contains(failures, "RabbitMQ:Username");
        StringAssert.Contains(failures, "RabbitMQ:Password");
    }

    [TestMethod]
    public void AddValidatedOptionsWhenDeclared_UndeclaredSection_IsNotValidated()
    {
        // ImageProcessingService has no "ConnectionStrings" section because it has no database.
        ValidateConnectionStringsWhenDeclared(("AllowedHosts", "*"));
    }

    [TestMethod]
    public void AddValidatedOptionsWhenDeclared_DeclaredButEmpty_Fails()
    {
        var exception = Assert.ThrowsExactly<OptionsValidationException>(
            () => ValidateConnectionStringsWhenDeclared(("ConnectionStrings:Default", ""))
        );

        StringAssert.Contains(string.Join(' ', exception.Failures), "ConnectionStrings:Default");
    }

    [TestMethod]
    public void AddValidatedOptionsWhenDeclared_OptionalBotTokenStaysOptional()
    {
        // An empty BotToken deliberately turns the services' Discord client off, so it must not fail.
        ValidateConnectionStringsWhenDeclared(
            ("ConnectionStrings:Default", "Host=localhost;Database=grillbot"),
            ("ConnectionStrings:BotToken", "")
        );
    }

    [TestMethod]
    public void AddValidatedOptionsWhenDeclared_ReturnsNullOnlyWhenNothingWasRegistered()
    {
        var empty = new ConfigurationBuilder().Build();
        var declared = new ConfigurationBuilder()
            .AddInMemoryCollection([new KeyValuePair<string, string?>("ConnectionStrings:Default", "Host=localhost")])
            .Build();

        Assert.IsNull(new ServiceCollection().AddValidatedOptionsWhenDeclared<ConnectionStringsOptions>(empty, ConnectionStringsOptions.SectionName));
        Assert.IsNotNull(new ServiceCollection().AddValidatedOptionsWhenDeclared<ConnectionStringsOptions>(declared, ConnectionStringsOptions.SectionName));
    }
}
