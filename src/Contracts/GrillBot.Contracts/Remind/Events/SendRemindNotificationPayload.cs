namespace GrillBot.Contracts.Remind.Events;

public sealed record SendRemindNotificationPayload(int RemindId, bool IsEarly);
