using GrillBot.Core.AsyncMessaging.Topology;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wolverine;
using Wolverine.RabbitMQ;
using Wolverine.RabbitMQ.Internal;

namespace GrillBot.Core.AsyncMessaging.Tests.Topology;

/// <summary>
/// Applies the topology to a real <see cref="WolverineOptions"/> - no broker involved - so a
/// mistake in the routing or binding calls surfaces here rather than at deploy time.
/// </summary>
[TestClass]
public class TopologyWiringTests
{
    [TestMethod]
    public void ApplyPublishing_DeclaresEveryExchange()
    {
        var options = CreateOptions();
        var expected = MessageTopology.Routes.Select(o => o.Exchange).Distinct().Order().ToList();

        MessageTopology.ApplyPublishing(options);

        var declared = Transport(options).Exchanges.Select(o => o.ExchangeName).Distinct().Order().ToList();
        CollectionAssert.AreEqual(expected, declared);
    }

    [TestMethod]
    public void ApplyPublishing_DeclaresEveryExchangeAsDirect()
    {
        var options = CreateOptions();

        MessageTopology.ApplyPublishing(options);

        Assert.IsTrue(Transport(options).Exchanges.All(o => o.ExchangeType == ExchangeType.Direct));
    }

    [TestMethod]
    public void ApplyBindings_DeclaresTheExchangesTheServiceConsumesFrom()
    {
        var options = CreateOptions();
        var expected = MessageTopology.Routes
            .Where(o => o.Queue == Queues.PointsService)
            .Select(o => o.Exchange)
            .Distinct()
            .Order()
            .ToList();

        MessageTopology.ApplyBindings(options, Queues.PointsService);

        var declared = Transport(options).Exchanges.Select(o => o.ExchangeName).Distinct().Order().ToList();
        CollectionAssert.AreEqual(expected, declared);
    }

    [TestMethod]
    public void ApplyBindings_ServiceWithoutHandlers_DeclaresNothing()
    {
        var options = CreateOptions();

        MessageTopology.ApplyBindings(options, Queues.ImageProcessingService);

        Assert.IsEmpty(Transport(options).Exchanges.ToList());
    }

    private static RabbitMqTransport Transport(WolverineOptions options)
        => options.Transports.GetOrCreate<RabbitMqTransport>();

    private static WolverineOptions CreateOptions()
    {
        var options = new WolverineOptions();
        options.UseRabbitMq(rabbit => rabbit.HostName = "localhost");

        return options;
    }
}
