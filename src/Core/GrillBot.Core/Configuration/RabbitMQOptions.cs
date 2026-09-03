using System.ComponentModel.DataAnnotations;

namespace GrillBot.Core.Configuration;

/// <summary>
/// The "RabbitMQ" section - the broker credentials every host connects with. The Wolverine
/// tuning that sits next to it lives in the "AsyncMessaging" section instead.
///
/// Nothing in this backend works without the broker: the bot and all services both publish
/// and consume integration events, so an unfilled credential is a startup failure and not a
/// degraded mode.
/// </summary>
public class RabbitMQOptions
{
    public const string SectionName = "RabbitMQ";

    [Required(ErrorMessage = "RabbitMQ:Hostname is required - it is the message broker every service publishes to and listens on.")]
    public string Hostname { get; set; } = null!;

    [Required(ErrorMessage = "RabbitMQ:Username is required - the broker rejects an anonymous connection.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "RabbitMQ:Password is required - the broker rejects an anonymous connection.")]
    public string Password { get; set; } = null!;
}
