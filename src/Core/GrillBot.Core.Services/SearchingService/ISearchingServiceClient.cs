using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Attributes;
using SearchingService.Models.Request;
using SearchingService.Models.Response;
using Refit;

namespace SearchingService;

[Service("Searching")]
public interface ISearchingServiceClient : IServiceClient
{
    [Post("/api/items/list")]
    Task<PaginatedResponse<SearchListItem>> GetSearchingListAsync(SearchingListRequest request, CancellationToken cancellationToken = default);

    [Get("/api/items/suggestions/{guildId}/{channelId}")]
    Task<List<SearchSuggestion>> GetSuggestionsAsync(
        string guildId,
        string channelId,
        [Header("Authorization")] string? authorizationToken = null,
        CancellationToken cancellationToken = default
    );

    [Delete("/api/items/remove/{id}")]
    Task RemoveSearchingAsync(
        long id,
        [Header("Authorization")] string? authorizationToken = null,
        CancellationToken cancellationToken = default
    );
}
