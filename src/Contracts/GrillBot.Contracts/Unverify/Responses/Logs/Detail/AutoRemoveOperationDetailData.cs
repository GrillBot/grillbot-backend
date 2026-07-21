namespace GrillBot.Contracts.Unverify.Responses.Logs.Detail;

public record AutoRemoveOperationDetailData(
    string Language,
    List<string> ReturnedRoles,
    List<ChannelOverride> ReturnedChannels
);