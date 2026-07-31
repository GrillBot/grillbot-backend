using Discord;

namespace GrillBot.Contracts.Emote.Events.Suggestions;

public sealed record EmoteSuggestionRequestPayload(
    string Name,
    string ReasonToAdd,
    byte[] Image,
    ulong GuildId,
    ulong FromUserId,
    DateTime CreatedAtUtc,
    bool IsAnimated,
    string Locale
)
{
    public static EmoteSuggestionRequestPayload Create(
        string name,
        string reasonToAdd,
        byte[] image,
        ulong guildId,
        ulong fromUserId,
        bool isAnimated,
        string locale
    ) => new(name, reasonToAdd, image, guildId, fromUserId, DateTime.UtcNow, isAnimated, locale);

    public static EmoteSuggestionRequestPayload Create(string name, string reasonToAdd, byte[] image, IGuild guild, IUser user, bool isAnimated, string locale)
        => Create(name, reasonToAdd, image, guild.Id, user.Id, isAnimated, locale);
}
