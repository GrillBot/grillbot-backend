namespace GrillBot.Contracts.Remind.Responses;

public record ReminderSuggestionItem(
    int RemindId,
    string FromUserId,
    string ToUserId,
    bool IsIncoming,
    DateTime NotifyAtUtc
);
