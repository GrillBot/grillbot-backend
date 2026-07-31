using Discord;

namespace GrillBot.Contracts.Emote.Events.Suggestions;

public sealed record EmoteSuggestionVoteMessageCreatedPayload(Guid SuggestionId, ulong MessageId)
{
    public static EmoteSuggestionVoteMessageCreatedPayload Create(Guid suggestionId, IMessage message)
        => new(suggestionId, message.Id);
}
