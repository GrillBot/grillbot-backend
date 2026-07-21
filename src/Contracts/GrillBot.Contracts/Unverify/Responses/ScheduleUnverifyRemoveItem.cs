namespace GrillBot.Contracts.Unverify.Responses;

public record ScheduleUnverifyRemoveItem(
    ulong GuildId,
    ulong UserId,
    int RolesToReturnCount,
    int ChannelsToReturnCount
);
