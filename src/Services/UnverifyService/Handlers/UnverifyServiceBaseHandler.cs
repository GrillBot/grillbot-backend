using GrillBot.Core.AsyncMessaging.Extensions;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using UnverifyService.Core.Entity;
using Wolverine;

namespace UnverifyService.Handlers;

/// <summary>
/// Unverify acts on someone's behalf, so its handlers only run for a message that carried the
/// caller's Authorization header. The old base class could gate that centrally because it owned
/// the consume method; Wolverine dispatches straight to the concrete handler, so each one now
/// opens with <see cref="TryGetCurrentUserAsync"/> instead.
/// </summary>
public abstract class UnverifyServiceBaseHandler(
    IServiceProvider serviceProvider
) : EventHandlerBaseWithDb<UnverifyContext>(serviceProvider)
{
    /// <summary>
    /// Resolves the caller from the envelope. Returns null and records the unauthorized attempt
    /// when the message arrived without a token, in which case the handler must stop.
    /// </summary>
    protected async Task<ICurrentUserProvider?> TryGetCurrentUserAsync(Envelope envelope)
    {
        var currentUser = envelope.CurrentUser();
        if (currentUser.IsLogged)
            return currentUser;

        await NotifyUnauthorizedExecution(envelope.MessageType ?? GetType().Name);
        return null;
    }
}
