namespace GrillBot.Core.AsyncMessaging.Topology;

/// <summary>
/// One message type, the exchange and routing key it is published with, and the
/// single queue that consumes it.
/// </summary>
/// <param name="MessageType">The contract record from GrillBot.Contracts.</param>
/// <param name="Exchange">Direct exchange, one per bounded context.</param>
/// <param name="RoutingKey">Unique within its exchange.</param>
/// <param name="Queue">A key from <see cref="Queues"/>.</param>
public sealed record MessageRoute(Type MessageType, string Exchange, string RoutingKey, string Queue);
