using Discord;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Managers.Random;
using GrillBot.Core.Services.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Core.Tests;

[TestClass]
public class CoreExtensionsTests
{
    [TestMethod]
    public void AddDiagnostic()
    {
        using var services = new ServiceCollection()
            .AddDiagnostic()
            .BuildServiceProvider();

        Assert.IsNotNull(services.GetService<DiagnosticsManager>());

        using var scope = services.CreateScope();
        Assert.IsNotNull(services.GetService<IDiagnosticsProvider>());
    }

    [TestMethod]
    public void AddCoreManagers()
    {
        using var services = new ServiceCollection()
            .AddCoreManagers()
            .BuildServiceProvider();

        Assert.IsNotNull(services.GetService<ICounterManager>());
        Assert.IsNotNull(services.GetService<IRandomManager>());
    }

    [TestMethod]
    public void AddFakeDiscordClient()
    {
        using var services = new ServiceCollection()
            .AddFakeDiscordClient()
            .AddFakeDiscordClient()
            .BuildServiceProvider();

        Assert.IsNull(services.GetService<IDiscordClient>());
    }
}
