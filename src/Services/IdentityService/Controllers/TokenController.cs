using GrillBot.Contracts.Identity;
using IdentityService.Options;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Globalization;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityService.Controllers;

/// <summary>
/// The token endpoint. OpenIddict authenticates the client and validates the request
/// before it reaches this controller; the controller only builds the identity to issue.
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class TokenController(IOptions<IdentityServerOptions> _options) : ControllerBase
{
    private const string Scheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme;

    [HttpPost("~/" + IdentityProtocol.TokenEndpoint)]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> ExchangeAsync()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsClientCredentialsGrantType())
            return SignIn(CreateClientPrincipal(request.ClientId!), Scheme);

        if (request.IsRefreshTokenGrantType())
        {
            // The principal is restored from the refresh token, claims included. Once IdentityService
            // owns the permission model, the claims are recomputed here instead.
            var result = await HttpContext.AuthenticateAsync(Scheme);
            if (result.Principal is null)
                return Reject(Errors.InvalidGrant, "The refresh token is no longer valid.");

            return SignIn(result.Principal, Scheme);
        }

        if (request.GrantType == IdentityProtocol.GrantTypes.Delegation)
            return CreateDelegatedResult(request);

        return Reject(Errors.UnsupportedGrantType, "The specified grant type is not supported.");
    }

    private ClaimsPrincipal CreateClientPrincipal(string clientId)
    {
        var identity = CreateIdentity();

        identity
            .SetClaim(Claims.Subject, clientId)
            .SetClaim(IdentityProtocol.ClaimTypes.NameIdentifier, clientId)
            .SetClaim(IdentityProtocol.ClaimTypes.Name, clientId)
            .SetClaim(IdentityProtocol.ClaimTypes.Channel, IdentityProtocol.Channels.Service);

        return Finish(identity, _options.Value.AccessTokenLifetime, offline: false);
    }

    private IActionResult CreateDelegatedResult(OpenIddictRequest request)
    {
        var subject = (string?)request.GetParameter(IdentityProtocol.Parameters.Subject);
        var name = (string?)request.GetParameter(IdentityProtocol.Parameters.Name);
        var channel = (string?)request.GetParameter(IdentityProtocol.Parameters.Channel);

        if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(channel))
            return Reject(Errors.InvalidRequest, "The subject, subject_name and channel parameters are required.");

        var lifetime = ResolveLifetime((string?)request.GetParameter(IdentityProtocol.Parameters.Lifetime));
        if (lifetime is null)
            return Reject(Errors.InvalidRequest, "The lifetime parameter must be a positive number of seconds.");

        var identity = CreateIdentity();
        identity
            .SetClaim(Claims.Subject, subject)
            .SetClaim(IdentityProtocol.ClaimTypes.NameIdentifier, subject)
            .SetClaim(IdentityProtocol.ClaimTypes.Name, name)
            .SetClaim(IdentityProtocol.ClaimTypes.Channel, channel);

        SetOptionalClaim(identity, IdentityProtocol.ClaimTypes.Role, request, IdentityProtocol.Parameters.Role);
        SetOptionalClaim(identity, IdentityProtocol.ClaimTypes.Permissions, request, IdentityProtocol.Parameters.Permissions);
        SetOptionalClaim(identity, IdentityProtocol.ClaimTypes.AvatarUrl, request, IdentityProtocol.Parameters.AvatarUrl);
        SetOptionalClaim(identity, IdentityProtocol.ClaimTypes.ThirdPartyKey, request, IdentityProtocol.Parameters.ThirdPartyKey);

        var offline = string.Equals((string?)request.GetParameter(IdentityProtocol.Parameters.Offline), "true", StringComparison.OrdinalIgnoreCase);
        return SignIn(Finish(identity, lifetime.Value, offline), Scheme);
    }

    private TimeSpan? ResolveLifetime(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return _options.Value.AccessTokenLifetime;

        if (!long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds) || seconds <= 0)
            return null;

        var requested = TimeSpan.FromSeconds(seconds);
        return requested > _options.Value.MaxDelegatedTokenLifetime ? _options.Value.MaxDelegatedTokenLifetime : requested;
    }

    private static void SetOptionalClaim(ClaimsIdentity identity, string claimType, OpenIddictRequest request, string parameter)
    {
        var value = (string?)request.GetParameter(parameter);
        if (!string.IsNullOrEmpty(value))
            identity.SetClaim(claimType, value);
    }

    private static ClaimsIdentity CreateIdentity()
    {
        return new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            IdentityProtocol.ClaimTypes.Name,
            IdentityProtocol.ClaimTypes.Role
        );
    }

    private ClaimsPrincipal Finish(ClaimsIdentity identity, TimeSpan lifetime, bool offline)
    {
        var principal = new ClaimsPrincipal(identity);

        principal.SetAudiences(_options.Value.Audience);
        principal.SetAccessTokenLifetime(lifetime);
        if (offline)
            principal.SetScopes(Scopes.OfflineAccess);

        principal.SetDestinations(static _ => [Destinations.AccessToken]);
        return principal;
    }

    private ForbidResult Reject(string error, string description)
    {
        var properties = new AuthenticationProperties(new Dictionary<string, string?>
        {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
        });

        return Forbid(properties, Scheme);
    }
}
