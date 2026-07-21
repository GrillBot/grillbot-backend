using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Attributes;
using InviteService.Models.Request;
using InviteService.Models.Response;
using Refit;

namespace InviteService;

[Service("InviteService")]
public interface IInviteServiceClient : IServiceClient
{
    [Post("/api/invite/cached-invites/list")]
    Task<PaginatedResponse<Invite>> GetCachedInvitesAsync(InviteListRequest request, CancellationToken cancellationToken = default);

    [Post("/api/invite/used-invites/list")]
    Task<PaginatedResponse<Invite>> GetUsedInvitesAsync(InviteListRequest request, CancellationToken cancellationToken = default);

    [Post("/api/invite/invite-uses/list")]
    Task<PaginatedResponse<InviteUse>> GetInviteUsesAsync(InviteUseListRequest request, CancellationToken cancellationToken = default);

    [Post("/api/invite/invite-user-uses/list")]
    Task<PaginatedResponse<UserInviteUse>> GetUserInviteUsesAsync(UserInviteUseListRequest request, CancellationToken cancellationToken = default);

    [Get("/api/invite/guild/{guildId}/count")]
    Task<int> GetInvitesCountOfGuildAsync(ulong guildId, CancellationToken cancellationToken = default);
}
