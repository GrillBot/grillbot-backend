using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using Microsoft.Extensions.Options;
using SearchingService.Core.Entity;
using GrillBot.Contracts.Searching.Events;
using SearchingService.Options;

namespace SearchingService.Handlers;

public class CreateSearchItemEventHandler(
    IServiceProvider serviceProvider,
    IOptions<AppOptions> _options
) : EventHandlerBaseWithDb<SearchingServiceContext>(serviceProvider)
{
    public async Task HandleAsync(SearchItemPayload payload, CancellationToken cancellationToken)
    {
        var created = DateTime.UtcNow;
        var entity = new SearchItem
        {
            ChannelId = payload.ChannelId,
            Content = payload.Content,
            CreatedAt = created,
            GuildId = payload.GuildId,
            UserId = payload.UserId,
            ValidTo = payload.ValidToUtc ?? DateTime.UtcNow.Add(_options.Value.DefaultItemValidity)
        };

        await DbContext.AddAsync(entity, cancellationToken);
        await ContextHelper.SaveChangesAsync(cancellationToken);
    }
}
