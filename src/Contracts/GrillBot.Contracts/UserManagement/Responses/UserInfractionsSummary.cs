namespace GrillBot.Contracts.UserManagement.Responses;

public record UserInfractionsSummary(
    Dictionary<string, int> WarningCount,
    Dictionary<string, int> UnverifyCount,
    Dictionary<string, int> TimeoutCount
);
