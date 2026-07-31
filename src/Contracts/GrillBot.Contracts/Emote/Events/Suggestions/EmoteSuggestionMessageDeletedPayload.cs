using Discord;

namespace GrillBot.Contracts.Emote.Events.Suggestions;

public sealed record EmoteSuggestionMessageDeletedPayload(ulong GuildId, ulong MessageId)
{
    public static EmoteSuggestionMessageDeletedPayload Create(IGuild guild, IMessage message)
        => new(guild.Id, message.Id);
}
