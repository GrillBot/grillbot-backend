namespace GrillBot.Contracts.Emote.Responses.EmoteSuggestions;

public record EmoteSuggestionVoteItem(
    string UserId,
    bool IsApproved,
    DateTime VotedAtUtc
);
