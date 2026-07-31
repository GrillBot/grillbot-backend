using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using Microsoft.Extensions.Caching.Distributed;
using GrillBot.Contracts.Rubbergod.Events.Pins;

namespace RubbergodService.Handlers.Pins;

public class ClearPinCacheEventHandler(
    IServiceProvider serviceProvider,
    IDistributedCache _cache
) : EventHandlerBase(serviceProvider)
{
    public async Task HandleAsync(ClearPinCachePayload message, CancellationToken cancellationToken)
    {
        await RemoveItemAsync("md", message);
        await RemoveItemAsync("json", message);
        return;
    }

    private async Task RemoveItemAsync(string type, ClearPinCachePayload payload)
    {
        var cacheKey = $"RubbergodService/PinCacheItem({payload.GuildId}, {payload.ChannelId}, {type})";

        byte[]? cacheItem;
        using (CreateCounter("Redis"))
            cacheItem = await _cache.GetAsync(cacheKey);

        if (cacheItem is null)
            return;

        using (CreateCounter("Redis"))
            await _cache.RemoveAsync(cacheKey);
    }
}
