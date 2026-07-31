namespace GrillBot.Contracts.Emote.Events;

public sealed record EmoteEventPayload(
    string GuildId,
    string UserId,
    string EmoteId,
    DateTime EventCreatedAt,
    bool IsIncrement
)
{
    public static EmoteEventPayload Increment(string guildId, string userId, string emoteId)
        => new(guildId, userId, emoteId, DateTime.UtcNow, true);

    public static EmoteEventPayload Decrement(string guildId, string userId, string emoteId)
        => new(guildId, userId, emoteId, DateTime.UtcNow, false);
}
