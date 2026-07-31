namespace GrillBot.Contracts.Points.Events;

public sealed record CreateTransactionPayload(
    string GuildId,
    DateTime CreatedAtUtc,
    string ChannelId,
    MessageInfo Message,
    ReactionInfo? Reaction = null
) : CreateTransactionBasePayload(GuildId);
