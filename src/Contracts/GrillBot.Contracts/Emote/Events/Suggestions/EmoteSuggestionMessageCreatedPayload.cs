using Discord;

namespace GrillBot.Contracts.Emote.Events.Suggestions;

public sealed record EmoteSuggestionMessageCreatedPayload(Guid SuggestionId, ulong MessageId)
{
    public static EmoteSuggestionMessageCreatedPayload Create(Guid suggestionId, IMessage message)
        => new(suggestionId, message.Id);
}
