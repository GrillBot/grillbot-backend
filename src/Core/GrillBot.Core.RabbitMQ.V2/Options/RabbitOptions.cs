namespace GrillBot.Core.RabbitMQ.V2.Options;

public class RabbitOptions
{
    public string Hostname { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public TimeSpan HeartBeat { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRetryCount { get; set; } = 5;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
}
