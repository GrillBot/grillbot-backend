using Discord;

namespace GrillBot.Contracts.Emote.Events.Suggestions;

public sealed record EmoteSuggestionApprovalChangePayload(Guid SuggestionId, bool IsApprovedForVote, ulong ApprovedByUserId)
{
    public static EmoteSuggestionApprovalChangePayload Create(Guid suggestionId, bool isApprovedForVote, IUser approvedBy)
        => new(suggestionId, isApprovedForVote, approvedBy.Id);
}
