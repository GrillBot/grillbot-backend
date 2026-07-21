using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Attributes;
using PointsService.Enums;
using PointsService.Models;
using PointsService.Models.Channels;
using PointsService.Models.Users;
using Refit;

namespace PointsService;

[Service("PointsService")]
public interface IPointsServiceClient : IServiceClient
{
    [Post("/api/admin/list")]
    Task<PaginatedResponse<TransactionItem>> GetTransactionListAsync(AdminListRequest request, CancellationToken cancellationToken = default);

    [Post("/api/chart")]
    Task<List<PointsChartItem>> GetChartDataAsync(AdminListRequest request, CancellationToken cancellationToken = default);

    [Get("/api/leaderboard/{guildId}")]
    Task<List<BoardItem>> GetLeaderboardAsync(
        string guildId,
        [Query] int skip,
        [Query] int count,
        [Query(delimiter: ",")] LeaderboardColumnFlag columns,
        [Query] LeaderboardSortOptions sort,
        CancellationToken cancellationToken = default
    );

    [Get("/api/leaderboard/{guildId}/count")]
    Task<int> GetLeaderboardCountAsync(string guildId, CancellationToken cancellationToken = default);

    [Post("/api/merge")]
    Task<MergeResult?> MergeValidTransctionsAsync(CancellationToken cancellationToken = default);

    [Get("/api/status/{guildId}/{userId}")]
    Task<PointsStatus> GetStatusOfPointsAsync(string guildId, string userId, CancellationToken cancellationToken = default);

    [Get("/api/status/{guildId}/{userId}/image")]
    Task<ImagePointsStatus?> GetImagePointsStatusAsync(string guildId, string userId, CancellationToken cancellationToken = default);

    [Post("/api/transaction/transfer")]
    Task TransferPointsAsync(TransferPointsRequest request, CancellationToken cancellationToken = default);

    [Post("/api/transaction/increment")]
    Task IncrementPointsAsync(IncrementPointsRequest request, CancellationToken cancellationToken = default);

    [Get("/api/transaction/{guildId}/{userId}")]
    Task<bool> ExistsAnyTransactionAsync(string guildId, string userId, CancellationToken cancellationToken = default);

    [Get("/api/diag/status")]
    Task<StatusInfo> GetStatusInfoAsync(CancellationToken cancellationToken = default);

    [Post("/api/user/list")]
    Task<PaginatedResponse<UserListItem>> GetUserListAsync(UserListRequest request, CancellationToken cancellationToken = default);

    [Get("/api/transaction/{guildId}/count")]
    Task<int> GetTransactionsCountForGuildAsync(string guildId, CancellationToken cancellationToken = default);

    [Get("/api/user/{guildId}/{userId}/info")]
    Task<UserInfo> GetUserInfoAsync(string guildId, string userId, CancellationToken cancellationToken = default);

    [Get("/api/channel/{guildId}/{channelId}/info")]
    Task<ChannelInfo> GetChannelInfoAsync(string guildId, string channelId, CancellationToken cancellationToken = default);
}
