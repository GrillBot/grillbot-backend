namespace GrillBot.Contracts.Unverify.Responses;

public record UnverifyInfo(
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string? FromUserId,
    bool IsSelfUnverify,
    string? Reason,
    int RemovedRolesCount,
    int KeepedRolesCount,
    int RemovedChannelsCount,
    int KeepedChannelsCount
);
