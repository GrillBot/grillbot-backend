namespace GrillBot.Contracts.Unverify.Responses.Logs;

public record UnverifyPreview(
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    int RemovedRolesCount,
    int KeepedRolesCount,
    int RemovedChnannelsCount,
    int KeepedChnannelsCount,
    string Reason
);
