using GrillBot.Contracts.Identity;

namespace IdentityService.Options;

/// <summary>
/// The <c>Identity</c> configuration section. Signing keys, the encryption key and client
/// secrets are sensitive and come from <c>/run/secrets</c>, never from appsettings.json.
/// </summary>
public class IdentityServerOptions
{
    public const string SectionName = "Identity";

    /// <summary>
    /// Issuer written to every token (<c>iss</c>). Must be the absolute URL under which the
    /// other hosts reach this service on the LAN, because the discovery document derives
    /// the JWKS address from it. Required outside Development.
    /// </summary>
    public string? Issuer { get; set; }

    public string Audience { get; set; } = IdentityProtocol.DefaultAudience;

    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(10);
    public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(30);

    /// <summary>
    /// How long a refresh token that was already redeemed is still accepted, so two requests
    /// refreshing at the same moment do not log the user out. After that, presenting it again
    /// is treated as theft and revokes every token of that sign-in. Zero disables the grace period.
    /// </summary>
    public TimeSpan RefreshTokenReuseLeeway { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Upper bound for the lifetime a client may request through the delegation grant.</summary>
    public TimeSpan MaxDelegatedTokenLifetime { get; set; } = TimeSpan.FromDays(1);

    public TimeSpan TokenPruneInterval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>Invalid and expired tokens older than this are deleted by the pruning worker.</summary>
    public TimeSpan TokenPruneThreshold { get; set; } = TimeSpan.FromDays(1);

    /// <summary>
    /// ES256 signing keys (PKCS#8 PEM). The first key signs new tokens; every key is
    /// published, so a retired key can stay listed until the tokens it signed expire.
    /// </summary>
    public List<SigningKeyOptions> SigningKeys { get; set; } = [];

    /// <summary>Base64 of a 256-bit key, used to encrypt refresh tokens.</summary>
    public string? EncryptionKey { get; set; }

    /// <summary>Registered clients, keyed by client ID. The registry in the database is synchronized with this list on startup.</summary>
    public Dictionary<string, ClientOptions> Clients { get; set; } = [];
}

public class SigningKeyOptions
{
    public string KeyId { get; set; } = "";
    public string Pem { get; set; } = "";
}

public class ClientOptions
{
    public string? DisplayName { get; set; }
    public string Secret { get; set; } = "";

    /// <summary>Grant types the client may use (<c>client_credentials</c>, <c>refresh_token</c>, the delegation grant).</summary>
    public List<string> GrantTypes { get; set; } = [];
}
