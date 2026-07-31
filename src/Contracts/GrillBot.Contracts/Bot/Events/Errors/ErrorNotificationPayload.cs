namespace GrillBot.Contracts.Bot.Events.Errors;

public sealed record ErrorNotificationPayload(string? Title, List<ErrorNotificationField> Fields, ulong? UserId);
