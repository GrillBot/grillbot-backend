namespace GrillBot.Contracts.Unverify.Responses.Logs;

public record AutoRemovePreview(
    int ReturnedRolesCount,
    int ReturnedChannelsCount
);
