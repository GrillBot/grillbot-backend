using GrillBot.Core.Metrics.Collectors;
using GrillBot.Core.Metrics.Components;

namespace GrillBot.Core.RabbitMQ.V2.Telemetry;

public class RabbitTelemetryCollector : ITelemetryCollector
{
    public TelemetryCounterContainer Consumer { get; } = new("rabbit_consumed_messages", "Count of messages consumed from each queue.");
    public TelemetryCounterContainer Producer { get; } = new("rabbit_published_messages", "Count of messages published to each queue.");

    public void IncrementConsumer(string topic, string queue)
        => Increment(Consumer, topic, queue);

    public void IncrementProducer(string topic, string queue)
        => Increment(Producer, topic, queue);

    private static void Increment(TelemetryCounterContainer container, string topic, string queue)
    {
        container.Increment(
            $"{topic}/{queue}",
            new Dictionary<string, object?>
            {
                ["topic"] = topic,
                ["queue"] = queue
            }
        );
    }

    public IEnumerable<TelemetryCollectorComponent> GetComponents()
    {
        yield return Consumer;
        yield return Producer;
    }
}
