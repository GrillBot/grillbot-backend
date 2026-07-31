namespace GrillBot.Contracts.Searching.Events;

public sealed record SearchItemPayload(
    string UserId,
    string GuildId,
    string ChannelId,
    string Content,
    DateTime? ValidToUtc
);
