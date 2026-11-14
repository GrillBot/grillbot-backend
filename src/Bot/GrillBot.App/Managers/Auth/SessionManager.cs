using GrillBot.App.Infrastructure.Options;
using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Contracts.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Buffers.Text;
using System.Security.Cryptography;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace GrillBot.App.Managers.Auth;

/// <summary>
/// Server-side browser sessions. The cookie carries only a random session ID; Redis holds
/// the access token under the ID's SHA-256 hash, so a copy of Redis is no list of live cookies.
/// </summary>
public class SessionManager(
    IDistributedCache _cache,
    JwtTokenManager _jwtTokenManager,
    IOptions<WebSessionOptions> _options,
    TimeProvider _timeProvider,
    IdentityAuditLog _auditLog
)
{
    private const string KeyPrefix = "auth:session:";
    private static readonly TimeSpan RenewBefore = TimeSpan.FromMinutes(1);

    private WebSessionOptions Options => _options.Value;

    /// <summary>Creates a session for a user who just signed in with Discord.</summary>
    /// <returns>The session ID for the cookie, or the reason the user may not sign in.</returns>
    public async Task<(string? sessionId, string? error)> StartAsync(IUser user, string language)
    {
        var token = await _jwtTokenManager.CreateTokenForUserAsync(user, language, lifetime: Options.AccessTokenLifetime);
        if (string.IsNullOrEmpty(token.AccessToken))
            return (null, token.ErrorMessage);

        var sessionId = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        var session = new WebSession(user.Id, language, token.AccessToken, token.ExpiresAt!.Value, _timeProvider.GetUtcNow());

        await SaveAsync(sessionId, session);
        return (sessionId, null);
    }

    /// <summary>
    /// Returns a valid access token for the session, renewing it when it is about to expire.
    /// Renewal recomputes role and permissions, so a revoked user loses access within one token lifetime.
    /// </summary>
    /// <returns><c>null</c> when the session does not exist, has expired or its user may no longer sign in.</returns>
    public async Task<string?> GetAccessTokenAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var session = await LoadAsync(sessionId, cancellationToken);
        if (session is null)
            return null;

        var now = _timeProvider.GetUtcNow();
        if (session.AccessTokenExpiresAt - now > RenewBefore)
            return session.AccessToken;

        var token = await _jwtTokenManager.RenewTokenForUserAsync(session.UserId, session.Language, Options.AccessTokenLifetime);
        if (string.IsNullOrEmpty(token.AccessToken))
        {
            await EndAsync(sessionId, cancellationToken);
            await _auditLog.WriteAsync(IdentityEventType.SessionRevoked, session.UserId, IdentityProtocol.Channels.Web, token.ErrorMessage);
            return null;
        }

        session = session with { AccessToken = token.AccessToken, AccessTokenExpiresAt = token.ExpiresAt!.Value };
        await SaveAsync(sessionId, session, cancellationToken);
        return session.AccessToken;
    }

    public Task EndAsync(string sessionId, CancellationToken cancellationToken = default)
        => _cache.RemoveAsync(CreateKey(sessionId), cancellationToken);

    /// <summary>Ends the session at the user's request.</summary>
    /// <returns>Whose session it was, or <c>null</c> when it no longer existed.</returns>
    public async Task<ulong?> SignOutAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var session = await LoadAsync(sessionId, cancellationToken);
        await EndAsync(sessionId, cancellationToken);
        return session?.UserId;
    }

    public CookieOptions CreateCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            IsEssential = true,
            MaxAge = Options.AbsoluteTimeout
        };
    }

    private async Task<WebSession?> LoadAsync(string sessionId, CancellationToken cancellationToken)
    {
        // A Get on a sliding entry also extends its idle expiration.
        var data = await _cache.GetAsync(CreateKey(sessionId), cancellationToken);
        return data is null ? null : JsonSerializer.Deserialize<WebSession>(data);
    }

    private Task SaveAsync(string sessionId, WebSession session, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = session.CreatedAt + Options.AbsoluteTimeout,
            SlidingExpiration = Options.IdleTimeout
        };

        return _cache.SetAsync(CreateKey(sessionId), JsonSerializer.SerializeToUtf8Bytes(session), options, cancellationToken);
    }

    private static string CreateKey(string sessionId)
        => KeyPrefix + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sessionId)));

    private sealed record WebSession(ulong UserId, string Language, string AccessToken, DateTimeOffset AccessTokenExpiresAt, DateTimeOffset CreatedAt);
}
