using GrillBot.App.Actions.Api.V3.Session;
using GrillBot.Data.Models.API.OAuth2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

/// <summary>
/// AdminWeb sign-in. The browser only ever holds an HttpOnly session cookie; the access token
/// stays on the server (see SessionTokenMiddleware).
/// </summary>
[ApiController]
[Route("api/session")]
[ApiExplorerSettings(GroupName = "v3")]
public class SessionController(IServiceProvider serviceProvider) : Core.Infrastructure.Actions.ControllerBase(serviceProvider)
{
    /// <summary>
    /// Start the sign-in: redirects to Discord, and after the user approves there, creates the
    /// session and redirects back to AdminWeb.
    /// </summary>
    /// <param name="returnUrl">Local AdminWeb path to return to after sign-in.</param>
    [HttpGet("login")]
    [Authorize(AuthenticationSchemes = "Discord")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public Task<IActionResult> LoginAsync([FromQuery] string? returnUrl)
        => ProcessAsync<StartSessionAction>(returnUrl);

    /// <summary>
    /// The signed-in user of the current session.
    /// </summary>
    /// <response code="200">Signed in.</response>
    /// <response code="204">Not signed in.</response>
    [HttpGet("me")]
    [AllowAnonymous]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(SessionInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> GetSessionAsync()
        => ProcessAsync<GetSessionAction>();

    /// <summary>
    /// Sign out: ends the session and deletes the cookie.
    /// </summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> LogoutAsync()
        => ProcessAsync<EndSessionAction>();
}
