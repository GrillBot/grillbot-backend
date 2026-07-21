using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Attributes;
using GrillBot.Models;
using Refit;
using System.Text.Json.Nodes;
using UnverifyService.Core.Enums;
using UnverifyService.Models.Request;
using UnverifyService.Models.Request.Keepables;
using UnverifyService.Models.Request.Logs;
using UnverifyService.Models.Request.Users;
using UnverifyService.Models.Response;
using UnverifyService.Models.Response.Guilds;
using UnverifyService.Models.Response.Keepables;
using UnverifyService.Models.Response.Logs;
using UnverifyService.Models.Response.Logs.Detail;
using UnverifyService.Models.Response.Users;

namespace UnverifyService;

[Service("Unverify")]
public interface IUnverifyServiceClient : IServiceClient
{
    // GuildController
    [Get("/api/guild/{guildId}")]
    Task<GuildInfo?> GetGuildInfoAsync(ulong guildId, CancellationToken cancellationToken = default);

    [Put("/api/guild/{guildId}")]
    Task<GuildInfo?> ModifyGuildAsync(ulong guildId, [Body] ModifyGuildRequest request, CancellationToken cancellationToken = default);

    // KeepablesController
    [Post("/api/keepables")]
    Task CreateKeepablesAsync([Body] List<CreateKeepableRequest> requests, CancellationToken cancellationToken = default);

    [Post("/api/keepables/list")]
    Task<PaginatedResponse<KeepableListItem>> GetKeepablesListAsync([Body] KeepablesListRequest request, CancellationToken cancellationToken = default);

    [Delete("/api/keepables/{group}")]
    Task DeleteKeepablesAsync(
        string group,
        [Query] string? name = null,
        CancellationToken cancellationToken = default
    );

    // LogsController
    [Post("/api/logs/list")]
    Task<PaginatedResponse<UnverifyLogItem>> GetUnverifyLogsAsync([Body] UnverifyLogListRequest request, CancellationToken cancellationToken = default);

    [Get("/api/logs/{id}")]
    Task<UnverifyLogDetail?> GetUnverifyLogDetailAsync(Guid id, CancellationToken cancellationToken = default);

    [Get("/api/logs/archive")]
    Task<ArchivationResult?> CreateArchivationDataAsync(CancellationToken cancellationToken = default);

    [Post("/api/logs/import-legacy")]
    Task ImportLegacyLogItemAsync([Body] JsonObject jsonData, CancellationToken cancellationToken = default);

    // StatisticsController
    [Get("/api/statistics/periodStats")]
    Task<Dictionary<string, long>> GetPeriodStatisticsAsync(
        [Query] string groupingKey,
        [Query] UnverifyOperationType operationType,
        CancellationToken cancellationToken = default
    );

    // UnverifyController
    [Post("/api/unverify/list")]
    Task<PaginatedResponse<ActiveUnverifyListItemResponse>> GetActiveUnverifyListAsync([Body] ActiveUnverifyListRequest request, CancellationToken cancellationToken = default);

    [Post("/api/unverify/list/current-user")]
    Task<PaginatedResponse<ActiveUnverifyListItemResponse>> GetCurrentUserUnverifyListAsync(
        [Body] ActiveUnverifyListRequest request,
        [Header("Authorization")] string? authorization = null,
        CancellationToken cancellationToken = default
    );

    [Post("/api/unverify/validate")]
    Task CheckUnverifyRequirementsAsync([Body] UnverifyRequest request, CancellationToken cancellationToken = default);

    [Delete("/api/unverify/{guildId}/{userId}")]
    Task<RemoveUnverifyResponse?> RemoveUnverifyAsync(
        ulong guildId,
        ulong userId,
        [Query] bool isForceRemove,
        [Header("Authorization")] string? authorization = null,
        CancellationToken cancellationToken = default
    );

    [Put("/api/unverify")]
    Task<LocalizedMessageContent?> UpdateUnverifyAsync(
        [Body] UpdateUnverifyRequest request,
        [Header("Authorization")] string? authorization = null,
        CancellationToken cancellationToken = default
    );

    [Get("/api/unverify/to-remove")]
    Task<List<ScheduleUnverifyRemoveItem>> GetUnverifiesToRemoveAsync(CancellationToken cancellationToken = default);

    [Get("/api/unverify/{guildId}/{userId}")]
    Task<UnverifyDetail?> GetActiveUnverifyDetailAsync(ulong guildId, ulong userId, CancellationToken cancellationToken = default);

    [Post("/api/unverify/recovery/validate")]
    Task CheckRecoveryRequirementsAsync(
        [Query] Guid? logId = null,
        [Query] long? logNumber = null,
        CancellationToken cancellationToken = default
    );

    // UserController
    [Put("/api/user/{userId}")]
    Task ModifyUserAsync(
        ulong userId,
        [Body] ModifyUserRequest request,
        [Header("Authorization")] string? authorization = null,
        CancellationToken cancellationToken = default
    );

    [Get("/api/user/{userId}")]
    Task<UserInfo?> GetUserInfoAsync(ulong userId, CancellationToken cancellationToken = default);
}