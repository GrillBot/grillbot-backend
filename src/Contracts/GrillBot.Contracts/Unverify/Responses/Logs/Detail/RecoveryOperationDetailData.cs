namespace GrillBot.Contracts.Unverify.Responses.Logs.Detail;

public record RecoveryOperationDetailData(
    List<string> ReturnedRoles,
    List<ChannelOverride> ReturnedChannels
);