using GrillBot.Contracts.Unverify.Enums;

namespace GrillBot.Contracts.Unverify.Responses.Logs.Detail;

public record UnverifyLogSimpleDetail(
    Guid Id,
    long LogNumber,
    string FromUserId,
    string ToUserId,
    DateTime CreatedAtUtc,
    UnverifyOperationType Type
);
