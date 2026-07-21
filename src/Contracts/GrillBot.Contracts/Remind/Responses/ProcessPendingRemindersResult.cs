namespace GrillBot.Contracts.Remind.Responses;

public record ProcessPendingRemindersResult(int RemindersCount, List<string> Messages);
