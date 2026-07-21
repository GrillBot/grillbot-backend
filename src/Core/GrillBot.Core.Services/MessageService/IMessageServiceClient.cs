using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Attributes;
using GrillBot.Contracts.Message.Requests.AutoReply;
using GrillBot.Contracts.Message.Responses.AutoReply;
using Refit;

namespace MessageService;

[Service("MessageService")]
public interface IMessageServiceClient : IServiceClient
{
    [Post("/api/autoreply")]
    Task<AutoReplyDefinition> CreateAutoReplyDefinition([Body] AutoReplyDefinitionRequest request, CancellationToken cancellationToken = default);

    [Get("/api/autoreply/{id}")]
    Task<AutoReplyDefinition> GetAutoReplyDefinition(Guid id, CancellationToken cancellationToken = default);

    [Post("/api/autoreply/list")]
    Task<PaginatedResponse<AutoReplyDefinition>> GetAutoReplyDefinitionListAsync(AutoReplyDefinitionListRequest request, CancellationToken cancellationToken = default);

    [Delete("/api/autoreply/{id}")]
    Task DeleteAutoReplyDefinitionAsync(Guid id, CancellationToken cancellationToken = default);

    [Put("/api/autoreply/{id}")]
    Task<AutoReplyDefinition> UpdateAutoReplyDefinitionAsync(Guid id, [Body] AutoReplyDefinitionRequest request, CancellationToken cancellationToken = default);
}
