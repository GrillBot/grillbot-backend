using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Attributes;
using GrillBot.Contracts.Emote.Requests;
using GrillBot.Contracts.Emote.Requests.EmoteSuggestions;
using GrillBot.Contracts.Emote.Requests.Guild;
using GrillBot.Contracts.Emote.Responses;
using GrillBot.Contracts.Emote.Responses.EmoteSuggestions;
using GrillBot.Contracts.Emote.Responses.Guild;
using Refit;

namespace Emote;

[Service("Emote")]
public interface IEmoteServiceClient : IServiceClient
{
    [Delete("/api/statistics/{guildId}/{emoteId}")]
    Task<int> DeleteStatisticsAsync(
        string guildId,
        string emoteId,
        [Query] string? userId = null,
        [Header("Authorization")] string? authorizationToken = null,
        CancellationToken cancellationToken = default
    );

    [Put("/api/statistics/{guildId}/{sourceEmoteId}/{destinationEmoteId}/merge")]
    Task<MergeStatisticsResult> MergeStatisticsAsync(
        string guildId,
        string sourceEmoteId,
        string destinationEmoteId,
        [Header("Authorization")] string? authorizationToken = null,
        CancellationToken cancellationToken = default
    );

    [Post("/api/statistics/emoteUsersUsage")]
    Task<PaginatedResponse<EmoteUserUsageItem>> GetUserEmoteUsageListAsync(EmoteUserUsageListRequest request, CancellationToken cancellationToken = default);

    [Post("/api/statistics/emoteStatistics")]
    Task<PaginatedResponse<EmoteStatisticsItem>> GetEmoteStatisticsListAsync(EmoteStatisticsListRequest request, CancellationToken cancellationToken = default);

    [Get("/api/emote/supported")]
    Task<List<EmoteDefinition>> GetSupportedEmotesListAsync([Query] string? guildId = null, CancellationToken cancellationToken = default);

    [Delete("/api/emote/supported/{guildId}/{emoteId}")]
    Task DeleteSupportedEmoteAsync(string guildId, string emoteId, CancellationToken cancellationToken = default);

    [Get("/api/emote/{guildId}/{emoteId}/info")]
    Task<EmoteInfo> GetEmoteInfoAsync(string guildId, string emoteId, CancellationToken cancellationToken = default);

    [Get("/api/statistics/count/{guildId}")]
    Task<long> GetStatisticsCountInGuildAsync(string guildId, CancellationToken cancellationToken = default);

    [Put("/api/guild/{guildId}")]
    Task UpdateGuildAsync(ulong guildId, GuildRequest request, CancellationToken cancellationToken = default);

    [Get("/api/guild/{guildId}")]
    Task<GuildData> GetGuildAsync(ulong guildId, CancellationToken cancellationToken = default);

    [Post("/api/emotesuggestions/list")]
    Task<PaginatedResponse<EmoteSuggestionItem>> GetEmoteSuggestionsAsync(EmoteSuggestionsListRequest request, CancellationToken cancellationToken = default);

    [Put("/api/emotesuggestions/approve/{suggestionId}")]
    Task SetSuggestionApprovalAsync(
        Guid suggestionId,
        [Query] bool isApproved,
        [Header("Authorization")] string? authorizationToken = null,
        CancellationToken cancellationToken = default
    );

    [Post("/api/emotesuggestions/{suggestionId}/votes")]
    Task<PaginatedResponse<EmoteSuggestionVoteItem>> GetSuggestionVotesAsync(Guid suggestionId, EmoteSuggestionVoteListRequest request, CancellationToken cancellationToken = default);

    [Post("/api/emotesuggestions/vote/{guildId}")]
    Task<int> StartSuggestionsVotingAsync(ulong guildId, CancellationToken cancellationToken = default);

    [Post("/api/emotesuggestions/vote/finish")]
    Task<int> FinishSuggestionVotesAsync(CancellationToken cancellationToken = default);

    [Get("/api/emotesuggestions/preview/{suggestionId}")]
    Task<Stream> GetEmoteSuggestionImagePreviewAsync(Guid suggestionId, CancellationToken cancellationToken = default);
}
