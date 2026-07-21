namespace GrillBot.Contracts.Unverify.Responses.Logs;

public record RemovePreview(
    int ReturnedRolesCount,
    int ReturnedChannelsCount,
    bool IsFromWeb,
    bool IsForcedRemoval
);
