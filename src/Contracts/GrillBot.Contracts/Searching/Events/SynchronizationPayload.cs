using GrillBot.Contracts.Searching.Events.Users;
using GrillBot.Core.RabbitMQ.V2.Messages;

namespace GrillBot.Contracts.Searching.Events;

public class SynchronizationPayload : IRabbitMessage
{
    public string Topic => "Searching";
    public string Queue => "Synchronization";

    public List<UserSynchronizationItem> Users { get; set; } = [];

    public SynchronizationPayload()
    {
    }

    public SynchronizationPayload(IEnumerable<UserSynchronizationItem> users)
    {
        Users = [.. users];
    }
}
