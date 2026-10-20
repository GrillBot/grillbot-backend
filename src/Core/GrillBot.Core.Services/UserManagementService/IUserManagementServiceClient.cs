using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Attributes;
using GrillBot.Contracts.UserManagement.Requests;
using GrillBot.Contracts.UserManagement.Responses;
using GrillBot.Core.Models.Pagination;
using Refit;

namespace UserManagementService;

[Service("UserManagementService")]
public interface IUserManagementServiceClient : IServiceClient
{
    [Get("/api/guild/{guildId}/users/with-nickname")]
    Task<Dictionary<string, string>> GetGuildUsersWithNicknameAsync(ulong guildId, CancellationToken cancellationToken = default);

    [Get("/api/user/{userId}")]
    Task<UserInfo> GetUserInfoAsync(ulong userId, CancellationToken cancellationToken = default);

    [Get("/api/infractions/dashboard")]
    Task<List<InfractionDashboardRow>> GetInfractionsDashboardAsync(CancellationToken cancellationToken = default);

    [Get("/api/infractions/count/{guildId}")]
    Task<int> GetInfractionsCountOfGuildAsync(string guildId, CancellationToken cancellationToken = default);

    [Post("/api/infractions/list")]
    Task<PaginatedResponse<InfractionItem>> GetInfractionsListAsync(InfractionListParams parameters, CancellationToken cancellationToken = default);

    [Get("/api/infractions/user/{userId}")]
    Task<UserInfractionsSummary> GetUserInfractionsSummaryAsync(string userId, [Query] string? guildId, CancellationToken cancellationToken = default);

    [Delete("/api/infractions")]
    Task DeleteInfractionAsync([Query] DeleteInfractionRequest request, CancellationToken cancellationToken = default);
}
