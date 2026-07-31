namespace GrillBot.Contracts.Points.Events;

public sealed record DeleteTransactionsPayload(string GuildId, string MessageId, string? ReactionId = null);
