using GrillBot.Contracts.Bot;

namespace GrillBot.Contracts.Unverify.Responses;

public record RemoveUnverifyResponse(
    LocalizedMessageContent Message,
    int ReturnedRolesCount,
    int ReturnedChannelsCount
);
