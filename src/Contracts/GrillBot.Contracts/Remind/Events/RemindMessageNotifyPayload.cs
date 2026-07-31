namespace GrillBot.Contracts.Remind.Events;

public sealed record RemindMessageNotifyPayload(int RemindId, string NotificationMessageId);
