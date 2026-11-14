using GrillBot.App.Infrastructure.Options;
using GrillBot.App.Managers.Auth;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Contracts.Identity;
using GrillBot.Core.Infrastructure.Actions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GrillBot.App.Actions.Api.V3.Session;

/// <summary>
/// Last step of the AdminWeb sign-in: the user has just come back from Discord. Creates the
/// server-side session, sets the session cookie, drops the temporary Discord cookie and
/// redirects to AdminWeb.
/// </summary>
public class StartSessionAction(
    ApiRequestContext _apiContext,
    SessionManager _sessionManager,
    ITextsManager _texts,
    IOptions<WebSessionOptions> _options,
    IdentityAuditLog _auditLog
) : ApiAction(_apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var returnUrl = GetOptionalParameter<string>(0);

        // The Discord identity was only needed for this hop; the session cookie replaces it.
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (ApiContext.LoggedUser is null)
        {
            var notFound = _texts["Auth/CreateToken/UserNotFound", ApiContext.Language];
            var discordUserId = ApiContext.GetUserId();
            await _auditLog.WriteAsync(IdentityEventType.SignInRefused, discordUserId == 0 ? null : discordUserId, IdentityProtocol.Channels.Web, notFound);
            return RedirectToLogin(notFound);
        }

        var (sessionId, error) = await _sessionManager.StartAsync(ApiContext.LoggedUser, ApiContext.Language);
        if (sessionId is null)
        {
            await _auditLog.WriteAsync(IdentityEventType.SignInRefused, ApiContext.LoggedUser.Id, IdentityProtocol.Channels.Web, error);
            return RedirectToLogin(error);
        }

        await _auditLog.WriteAsync(IdentityEventType.SignIn, ApiContext.LoggedUser.Id, IdentityProtocol.Channels.Web);
        HttpContext.Response.Cookies.Append(_options.Value.CookieName, sessionId, _sessionManager.CreateCookieOptions());
        return Redirect(SafeReturnPath(returnUrl));
    }

    private ApiResult RedirectToLogin(string? error)
        => Redirect($"/login?loginError={Uri.EscapeDataString(error ?? "")}");

    private ApiResult Redirect(string path)
    {
        var url = _options.Value.FrontendUrl.TrimEnd('/') + path;
        return new ApiResult(StatusCodes.Status302Found, new RedirectResult(url));
    }

    /// <summary>Only a local path is accepted, so the sign-in cannot be turned into an open redirect.</summary>
    private static string SafeReturnPath(string? returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl) || !returnUrl.StartsWith('/') || returnUrl.StartsWith("//") || returnUrl.StartsWith("/\\") || returnUrl.Contains("://"))
            return "/";

        return returnUrl;
    }
}
