namespace GrillBot.Contracts.Unverify.Responses;

public record UnverifyDetail(
    Guid Id,
    long LogNumber,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    string FromUserId,
    string? Reason,
    string Language,
    bool KeepMutedRole,
    List<string> RemovedRoles,
    List<string> KeepedRoles,
    List<ChannelOverride> RemovedChannels,
    List<ChannelOverride> KeepedChannels
);
