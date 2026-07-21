using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Attributes;
using UserMeasures.Models.Dashboard;
using UserMeasures.Models.Measures;
using UserMeasures.Models.User;
using Refit;

namespace UserMeasures;

[Service("UserMeasures")]
public interface IUserMeasuresServiceClient : IServiceClient
{
    [Get("/api/dashboard")]
    Task<List<DashboardRow>> GetDashboardDataAsync(CancellationToken cancellationToken = default);

    [Get("/api/info/count/{guildId}")]
    Task<int> GetItemsCountOfGuildAsync(string guildId, CancellationToken cancellationToken = default);

    [Post("/api/measures/list")]
    Task<PaginatedResponse<MeasuresItem>> GetMeasuresListAsync(MeasuresListParams parameters, CancellationToken cancellationToken = default);

    [Get("/api/user/{userId}")]
    Task<UserInfo> GetUserInfoAsync(string userId, [Query] string? guildId, CancellationToken cancellationToken = default);

    [Delete("/api/measures")]
    Task DeleteMeasureAsync([Query] DeleteMeasuresRequest request, CancellationToken cancellationToken = default);
}
