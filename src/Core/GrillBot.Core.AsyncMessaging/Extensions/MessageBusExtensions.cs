using GrillBot.Core.Infrastructure.Auth;
using Wolverine;

namespace GrillBot.Core.AsyncMessaging.Extensions;

public static class MessageBusExtensions
{
    /// <summary>
    /// Publishes a message carrying the current caller's Authorization header, so the
    /// handler on the other side can act on their behalf.
    /// </summary>
    public static ValueTask PublishAsync<T>(this IMessageBus bus, T message, ICurrentUserProvider currentUser) where T : notnull
    {
        var headers = currentUser.ToDictionary();
        if (headers is null || headers.Count == 0)
            return bus.PublishAsync(message);

        var options = new DeliveryOptions();
        foreach (var (key, value) in headers)
            options.WithHeader(key, value);

        return bus.PublishAsync(message, options);
    }

    /// <summary>
    /// Publishes every message in the batch. Wolverine batches on the wire itself, so this
    /// is just a loop - there is no separate batch API to reach for.
    /// </summary>
    public static async ValueTask PublishAllAsync<T>(this IMessageBus bus, IEnumerable<T> messages, ICurrentUserProvider? currentUser = null) where T : notnull
    {
        foreach (var message in messages)
        {
            if (currentUser is null)
                await bus.PublishAsync(message);
            else
                await bus.PublishAsync(message, currentUser);
        }
    }
}
