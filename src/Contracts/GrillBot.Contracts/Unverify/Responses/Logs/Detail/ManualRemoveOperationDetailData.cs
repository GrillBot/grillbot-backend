namespace GrillBot.Contracts.Unverify.Responses.Logs.Detail;

public record ManualRemoveOperationDetailData(
    bool IsFromWeb,
    string Language,
    bool IsForceRemove,
    List<string> ReturnedRoles,
    List<ChannelOverride> ReturnedChannels
);