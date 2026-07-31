using GrillBot.Core.Infrastructure.Auth;
using Wolverine;

namespace GrillBot.Core.AsyncMessaging.Extensions;

public static class EnvelopeExtensions
{
    /// <summary>
    /// Rebuilds the caller's identity from the envelope headers. The publisher forwards its
    /// Authorization header with the message (see MessageBusExtensions), so a handler can
    /// tell who triggered the work without a REST hop.
    /// </summary>
    public static ICurrentUserProvider CurrentUser(this Envelope envelope)
        => new CurrentUserProvider(envelope.Headers.ToDictionary(o => o.Key, o => o.Value ?? ""));
}
