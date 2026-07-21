namespace GrillBot.Contracts.Emote.Responses.Guild;

public record GuildData(
    string? SuggestionChannelId,
    string? VoteChannelId,
    TimeSpan VoteTime
);
