using GrillBot.Contracts.Unverify.Enums;

namespace GrillBot.Contracts.Unverify.Responses.Logs;

public record UnverifyLogItem(
    Guid Id,
    Guid? ParentItemId,
    long LogNumber,
    UnverifyOperationType Type,
    string GuildId,
    string FromUserId,
    string ToUserId,
    DateTime CreatedAtUtc,
    object? Preview
);
