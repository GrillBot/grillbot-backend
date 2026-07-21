namespace GrillBot.Contracts.Searching.Responses;

public record SearchListItem(
    long Id,
    string UserId,
    string GuildId,
    string ChannelId,
    string Content,
    DateTime CreatedAtUtc,
    DateTime ValidToUtc,
    bool IsInvalid,
    bool IsDeleted
);
