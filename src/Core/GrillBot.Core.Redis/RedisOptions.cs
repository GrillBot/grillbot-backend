using System.ComponentModel.DataAnnotations;

namespace GrillBot.Core.Redis;

/// <summary>
/// The "Redis" section. Only the hosts that declare it use a Redis server - the bot and the
/// four services that cache across replicas - and for those the connection is opened with
/// <c>AbortOnConnectFail = true</c>, so an empty endpoint is a hard failure the first time
/// the cache is touched. Validating it at startup moves that failure to where it belongs.
/// </summary>
public class RedisOptions
{
    public const string SectionName = "Redis";

    [Required(ErrorMessage = "Redis:Endpoint is required - it is the host:port of the Redis server backing the distributed cache.")]
    public string Endpoint { get; set; } = null!;

    /// <summary>
    /// Optional: a Redis server without <c>requirepass</c> accepts an empty password.
    /// </summary>
    public string? Password { get; set; }
}
