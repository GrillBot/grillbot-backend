namespace GrillBot.Contracts.Bot.Events.Errors;

public sealed record ErrorNotificationField(string Key, string Value, bool IsInline);
