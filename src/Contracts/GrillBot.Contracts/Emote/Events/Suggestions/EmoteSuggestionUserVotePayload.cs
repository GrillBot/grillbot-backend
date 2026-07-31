using Discord;

namespace GrillBot.Contracts.Emote.Events.Suggestions;

public sealed record EmoteSuggestionUserVotePayload(Guid SuggestionId, bool IsApproved, ulong UserId)
{
    public static EmoteSuggestionUserVotePayload Create(Guid suggestionId, bool isApproved, IUser user)
        => new(suggestionId, isApproved, user.Id);
}
