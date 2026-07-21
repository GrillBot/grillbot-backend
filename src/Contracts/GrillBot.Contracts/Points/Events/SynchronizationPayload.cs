using GrillBot.Core.RabbitMQ.V2.Messages;
using GrillBot.Contracts.Points.Channels;
using GrillBot.Contracts.Points.Users;

namespace GrillBot.Contracts.Points.Events;

public class SynchronizationPayload : IRabbitMessage
{
    public string Topic => "Points";
    public string Queue => "Synchronization";

    public string GuildId { get; set; } = null!;
    public List<ChannelSyncItem> Channels { get; set; } = [];
    public List<UserSyncItem> Users { get; set; } = [];

    public SynchronizationPayload()
    {
    }

    public SynchronizationPayload(string guildId, IEnumerable<ChannelSyncItem> channels, IEnumerable<UserSyncItem> users)
    {
        GuildId = guildId;
        Channels = [.. channels];
        Users = [.. users];
    }
}
