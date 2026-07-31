using System.Reflection;
using System.Text.Json;
using GrillBot.Core.AsyncMessaging.Topology;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Core.AsyncMessaging.Tests.Topology;

/// <summary>
/// The topology is the one place a message type is tied to the broker. A type that never
/// gets a route is published into nothing and silently lost, so these tests guard the table
/// rather than any behaviour.
/// </summary>
[TestClass]
public class MessageTopologyTests
{
    [TestMethod]
    public void Routes_CoverEveryIntegrationEvent()
    {
        var routed = MessageTopology.Routes.Select(o => o.MessageType).ToHashSet();

        var events = typeof(Contracts.Points.Events.CreateTransactionPayload).Assembly
            .GetTypes()
            .Where(o => o is { IsClass: true, IsAbstract: false, IsPublic: true })
            .Where(o => o.Namespace?.Contains(".Events") == true)
            .Where(IsIntegrationEvent)
            .ToList();

        var missing = events.Where(o => !routed.Contains(o)).Select(o => o.FullName).Order().ToList();

        Assert.IsEmpty(missing, $"Message types without a route: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public void Routes_HaveUniqueRoutingKeyPerExchange()
    {
        var duplicates = MessageTopology.Routes
            .GroupBy(o => (o.Exchange, o.RoutingKey))
            .Where(o => o.Count() > 1)
            .Select(o => $"{o.Key.Exchange}/{o.Key.RoutingKey}")
            .ToList();

        Assert.IsEmpty(duplicates, $"Routing keys used more than once: {string.Join(", ", duplicates)}");
    }

    [TestMethod]
    public void Routes_HaveUniqueMessageType()
    {
        var duplicates = MessageTopology.Routes
            .GroupBy(o => o.MessageType)
            .Where(o => o.Count() > 1)
            .Select(o => o.Key.FullName!)
            .ToList();

        Assert.IsEmpty(duplicates, $"Message types routed more than once: {string.Join(", ", duplicates)}");
    }

    [TestMethod]
    public void Routes_TargetKnownQueues()
    {
        var unknown = MessageTopology.Routes
            .Select(o => o.Queue)
            .Where(o => !Queues.All.Contains(o))
            .Distinct()
            .ToList();

        Assert.IsEmpty(unknown, $"Unknown queues: {string.Join(", ", unknown)}");
    }

    [TestMethod]
    public void Queues_MatchDeployables()
    {
        using var stream = File.OpenRead("deployables.json");
        using var document = JsonDocument.Parse(stream);

        var deployables = document.RootElement.GetProperty("deployables")
            .EnumerateObject()
            .Select(o => o.Name)
            .ToHashSet();

        var unknown = Queues.All.Where(o => !deployables.Contains(o)).Order().ToList();

        Assert.IsEmpty(unknown, $"Queues that are not deployables: {string.Join(", ", unknown)}");
    }

    /// <summary>
    /// An integration event is a record whose name ends in Payload or Message and that is not
    /// a nested item of another payload (those live in child namespaces such as Events.Users).
    /// </summary>
    private static bool IsIntegrationEvent(Type type)
    {
        if (!type.Name.EndsWith("Payload") && !type.Name.EndsWith("Message"))
            return false;

        // Records get a compiler-generated clone method; plain DTO classes do not.
        if (type.GetMethod("<Clone>$", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) is null)
            return false;

        return type.Namespace!.EndsWith(".Events")
            || type.Namespace!.Contains(".Events.Create")
            || type.Namespace!.Contains(".Events.Errors")
            || type.Namespace!.Contains(".Events.Guild")
            || type.Namespace!.Contains(".Events.Karma")
            || type.Namespace!.Contains(".Events.Messages")
            || type.Namespace!.Contains(".Events.Pins")
            || type.Namespace!.Contains(".Events.Recalculation")
            || type.Namespace!.Contains(".Events.Suggestions");
    }
}
