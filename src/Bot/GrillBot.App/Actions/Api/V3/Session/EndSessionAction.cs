using GrillBot.App.Infrastructure.Options;
using GrillBot.App.Managers.Auth;
using GrillBot.Common.Models;
using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Contracts.Identity;
using GrillBot.Core.Infrastructure.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace GrillBot.App.Actions.Api.V3.Session;

/// <summary>Sign-out: deletes the server-side session and the cookie. Always 204.</summary>
public class EndSessionAction(
    ApiRequestContext _apiContext,
    SessionManager _sessionManager,
    IOptions<WebSessionOptions> _options,
    IdentityAuditLog _auditLog
) : ApiAction(_apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var cookieName = _options.Value.CookieName;

        if (HttpContext.Request.Cookies.TryGetValue(cookieName, out var sessionId) && !string.IsNullOrEmpty(sessionId))
        {
            var userId = await _sessionManager.SignOutAsync(sessionId, CancellationToken);
            if (userId is not null)
                await _auditLog.WriteAsync(IdentityEventType.SignOut, userId, IdentityProtocol.Channels.Web);
        }

        HttpContext.Response.Cookies.Delete(cookieName, _sessionManager.CreateCookieOptions());
        return new ApiResult(StatusCodes.Status204NoContent);
    }
}
