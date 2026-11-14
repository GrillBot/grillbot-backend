using GrillBot.App.Infrastructure.Options;
using GrillBot.App.Managers.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GrillBot.App.Infrastructure.Auth;

/// <summary>
/// Turns the AdminWeb session cookie into the request's bearer token. Everything after this
/// middleware - JwtBearer, the controllers, <c>CurrentUserProvider</c>, calls to services -
/// sees an ordinary <c>Authorization</c> header and needs no change.
/// </summary>
public class SessionTokenMiddleware(RequestDelegate _next, IOptions<WebSessionOptions> _options, IHostEnvironment _environment)
{
    private static readonly HashSet<string> SafeMethods = new(["GET", "HEAD", "OPTIONS"], StringComparer.OrdinalIgnoreCase);

    public async Task InvokeAsync(HttpContext context, SessionManager sessionManager)
    {
        var cookieName = _options.Value.CookieName;

        // An explicit Authorization header (third-party API keys, legacy admins, Swagger) wins.
        if (context.Request.Headers.Authorization.Count > 0 || !context.Request.Cookies.TryGetValue(cookieName, out var sessionId) || string.IsNullOrEmpty(sessionId))
        {
            await _next(context);
            return;
        }

        if (!IsAllowedOrigin(context.Request))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        var accessToken = await sessionManager.GetAccessTokenAsync(sessionId, context.RequestAborted);
        if (accessToken is null)
            context.Response.Cookies.Delete(cookieName, sessionManager.CreateCookieOptions());
        else
            context.Request.Headers.Authorization = $"Bearer {accessToken}";

        await _next(context);
    }

    /// <summary>
    /// CSRF protection on top of SameSite=Strict: a state-changing request carried by the cookie
    /// must come from the same origin. In Development AdminWeb runs on another port (same site).
    /// </summary>
    private bool IsAllowedOrigin(HttpRequest request)
    {
        if (SafeMethods.Contains(request.Method))
            return true;

        var fetchSite = request.Headers["Sec-Fetch-Site"].ToString();
        if (string.IsNullOrEmpty(fetchSite) || fetchSite == "same-origin")
            return true;

        return fetchSite == "same-site" && _environment.IsDevelopment();
    }
}
