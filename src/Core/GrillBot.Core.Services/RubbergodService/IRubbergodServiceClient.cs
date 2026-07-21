using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Attributes;
using RubbergodService.Models.Help;
using RubbergodService.Models.Karma;
using Refit;

namespace RubbergodService;

[Service("RubbergodService")]
public interface IRubbergodServiceClient : IServiceClient
{
    [Get("/api/karma")]
    Task<PaginatedResponse<UserKarma>> GetKarmaPageAsync([Query] PaginatedParams parameters, CancellationToken cancellationToken = default);

    [Get("/api/pins/{guildId}/{channelId}")]
    Task<HttpContent> GetPinsAsync(ulong guildId, ulong channelId, [Query] bool markdown, CancellationToken cancellationToken = default);

    [Get("/api/help/slashcommands")]
    Task<Dictionary<string, Cog>> GetSlashCommandsAsync(CancellationToken cancellationToken = default);
}
