namespace GrillBot.Contracts.Points.Events;

public sealed record CreateTransactionAdminPayload(string GuildId, string UserId, int Amount)
    : CreateTransactionBasePayload(GuildId);
