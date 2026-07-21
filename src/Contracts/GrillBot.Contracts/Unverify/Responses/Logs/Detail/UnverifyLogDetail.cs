using GrillBot.Contracts.Unverify.Enums;

namespace GrillBot.Contracts.Unverify.Responses.Logs.Detail;

public record UnverifyLogDetail(
    Guid Id,
    long LogNumber,
    UnverifyLogSimpleDetail? ParentInfo,
    UnverifyOperationType OperationType,
    string GuildId,
    string FromUserId,
    string ToUserId,
    DateTime CreatedAtUtc,
    object? Data
);
