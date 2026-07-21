namespace GrillBot.Contracts.Unverify.Responses;

public record ChannelOverride(
    string ChannelId,
    List<string> AllowValues,
    List<string> DenyValues
);
