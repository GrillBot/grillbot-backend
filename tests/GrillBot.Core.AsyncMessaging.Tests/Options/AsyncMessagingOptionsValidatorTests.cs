using GrillBot.Core.AsyncMessaging.Options;
using GrillBot.Core.AsyncMessaging.Topology;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Core.AsyncMessaging.Tests.Options;

[TestClass]
public class AsyncMessagingOptionsValidatorTests
{
    private static AsyncMessagingOptions ValidOptions() => new()
    {
        QueueName = Queues.PointsService,
        ListenerCount = 1,
        MaximumParallelMessages = 1,
        DefaultExecutionTimeout = TimeSpan.FromHours(4),
        DeadLetterQueueExpiration = TimeSpan.FromDays(14),
        RetryCooldowns = [TimeSpan.FromSeconds(1)]
    };

    private static ValidateOptionsResult Validate(Action<AsyncMessagingOptions> mutate)
    {
        var options = ValidOptions();
        mutate(options);

        return new AsyncMessagingOptionsValidator().Validate(Microsoft.Extensions.Options.Options.DefaultName, options);
    }

    [TestMethod]
    public void ShippedDefaultsAreValid()
    {
        Assert.IsTrue(Validate(_ => { }).Succeeded);
    }

    [TestMethod]
    public void EveryDeployableQueueIsAccepted()
    {
        foreach (var queue in Queues.All)
            Assert.IsTrue(Validate(o => o.QueueName = queue).Succeeded, queue);
    }

    [TestMethod]
    public void EmptyQueueNameFails()
    {
        var result = Validate(o => o.QueueName = "");

        Assert.IsTrue(result.Failed);
        StringAssert.Contains(result.FailureMessage!, "AsyncMessaging:QueueName");
    }

    [TestMethod]
    public void UnknownQueueNameFails()
    {
        var result = Validate(o => o.QueueName = "not_a_deployable");

        Assert.IsTrue(result.Failed);
        StringAssert.Contains(result.FailureMessage!, "docker/deployables.json");
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void NonPositiveListenerCountFails(int listenerCount)
    {
        Assert.IsTrue(Validate(o => o.ListenerCount = listenerCount).Failed);
    }

    [TestMethod]
    public void NonPositiveMaximumParallelMessagesFails()
    {
        Assert.IsTrue(Validate(o => o.MaximumParallelMessages = 0).Failed);
    }

    [TestMethod]
    public void NonPositiveTimeoutsFail()
    {
        Assert.IsTrue(Validate(o => o.DefaultExecutionTimeout = TimeSpan.Zero).Failed);
        Assert.IsTrue(Validate(o => o.DeadLetterQueueExpiration = TimeSpan.Zero).Failed);
    }

    [TestMethod]
    public void EmptyRetryCooldownsFail()
    {
        Assert.IsTrue(Validate(o => o.RetryCooldowns = []).Failed);
    }

    [TestMethod]
    public void NegativeRetryCooldownFails()
    {
        Assert.IsTrue(Validate(o => o.RetryCooldowns = [TimeSpan.FromSeconds(-1)]).Failed);
    }

    [TestMethod]
    public void EveryBrokenValueIsReportedTogether()
    {
        var result = Validate(o =>
        {
            o.QueueName = "";
            o.ListenerCount = 0;
            o.RetryCooldowns = [];
        });

        Assert.IsTrue(result.Failed);
        Assert.AreEqual(3, result.Failures!.Count());
    }
}
