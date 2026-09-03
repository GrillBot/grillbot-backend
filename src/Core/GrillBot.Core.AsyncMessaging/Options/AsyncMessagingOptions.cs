namespace GrillBot.Core.AsyncMessaging.Options;

/// <summary>
/// The "AsyncMessaging" configuration section. Broker credentials are not here -
/// those stay in "RabbitMQ" (Hostname/Username/Password) where they have always been.
/// </summary>
public class AsyncMessagingOptions
{
    public const string SectionName = "AsyncMessaging";

    /// <summary>
    /// The single queue this application listens to. Must match the application's key in
    /// docker/deployables.json, which is also its image tag, Swarm service name and health path.
    /// It is used as Wolverine's ServiceName too, so the OpenTelemetry meter becomes
    /// "Wolverine:&lt;QueueName&gt;".
    /// </summary>
    public string QueueName { get; set; } = null!;

    public int ListenerCount { get; set; } = 1;

    public int MaximumParallelMessages { get; set; } = 1;

    public TimeSpan DefaultExecutionTimeout { get; set; } = TimeSpan.FromHours(4);

    public TimeSpan DeadLetterQueueExpiration { get; set; } = TimeSpan.FromDays(14);

    /// <summary>
    /// Inline retry delays applied before a message is moved to the error queue.
    /// </summary>
    public TimeSpan[] RetryCooldowns { get; set; } =
    [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(30)
    ];

    public bool PurgeOnStartup { get; set; }
}
