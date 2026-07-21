using GrillBot.Core.RabbitMQ.V2.Messages;

namespace GrillBot.Contracts.Remind.Events;

public class RemindMessageNotifyPayload : IRabbitMessage
{
    public string Topic => "Remind";
    public string Queue => "RemindMessageNotify";

    public int RemindId { get; set; }
    public string NotificationMessageId { get; set; } = null!;

    public RemindMessageNotifyPayload()
    {
    }

    public RemindMessageNotifyPayload(int remindId, string notificationMessageId)
    {
        RemindId = remindId;
        NotificationMessageId = notificationMessageId;
    }
}
