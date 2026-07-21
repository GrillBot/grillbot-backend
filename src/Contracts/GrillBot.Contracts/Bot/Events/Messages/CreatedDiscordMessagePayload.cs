using GrillBot.Core.RabbitMQ.V2.Messages;

namespace GrillBot.Contracts.Bot.Events.Messages;

public class CreatedDiscordMessagePayload : IRabbitMessage
{
    public string Topic => "GrillBot";
    public string Queue => "CreatedMessage";

    public string? GuildId { get; set; }
    public string ChannelId { get; set; } = null!;
    public string MessageId { get; set; } = null!;
    public string ServiceId { get; set; } = null!;
    public Dictionary<string, string> ServiceData { get; set; } = [];

    public CreatedDiscordMessagePayload()
    {
    }

    public CreatedDiscordMessagePayload(string? guildId, string channelId, string messageId, string serviceId, Dictionary<string, string> serviceData)
    {
        GuildId = guildId;
        ChannelId = channelId;
        MessageId = messageId;
        ServiceId = serviceId;
        ServiceData = serviceData;
    }
}
