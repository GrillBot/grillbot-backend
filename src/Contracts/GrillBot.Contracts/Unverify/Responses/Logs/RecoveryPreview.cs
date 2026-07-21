namespace GrillBot.Contracts.Unverify.Responses.Logs;

public record RecoveryPreview(
    int ReturnedRolesCount,
    int ReturnedChannelsCount
);
