using GrillBot.Contracts.Identity.Requests;

namespace GrillBot.Core.Services.IdentityService;

/// <summary>Requests tokens from IdentityService's token endpoint.</summary>
public interface IIdentityTokenClient
{
    /// <summary>Issues a token on behalf of a subject whose claims this client supplies (delegation grant).</summary>
    /// <exception cref="IdentityTokenException">IdentityService refused the request.</exception>
    Task<IdentityToken> RequestDelegatedTokenAsync(DelegatedTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>Issues a token for this client itself (client credentials grant).</summary>
    /// <exception cref="IdentityTokenException">IdentityService refused the request.</exception>
    Task<IdentityToken> RequestClientCredentialsTokenAsync(CancellationToken cancellationToken = default);
}

public record IdentityToken(string AccessToken, DateTimeOffset ExpiresAt, string? RefreshToken);
