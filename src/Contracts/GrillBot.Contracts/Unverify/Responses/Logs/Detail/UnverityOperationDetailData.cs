namespace GrillBot.Contracts.Unverify.Responses.Logs.Detail;

public record UnverityOperationDetailData(
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    long DurationMilliseconds,
    string Reason,
    string Language,
    bool IsMutedRoleKeeped,
    List<string> RemovedRoles,
    List<string> KeepedRoles,
    List<ChannelOverride> RemovedChannels,
    List<ChannelOverride> KeepedChannels
);
