using GrillBot.Core.RabbitMQ.V2.Messages;

namespace GrillBot.Contracts.Unverify.Events;

public class SynchronizationMessage : IRabbitMessage
{
    public string Topic => "Unverify";
    public string Queue => "Synchronization";

    public List<UserSyncMessage> Users { get; set; } = [];

    public SynchronizationMessage()
    {
    }

    public SynchronizationMessage(IEnumerable<UserSyncMessage> users)
    {
        Users = [.. users];
    }
}
