using System.Text.Json.Serialization;

namespace GrillBot.Contracts.Identity.Responses;

/// <summary>
/// The OAuth 2.0 token endpoint response (RFC 6749, section 5.1 and 5.2) as
/// IdentityService returns it. Either <see cref="AccessToken"/> or <see cref="Error"/> is set.
/// </summary>
public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    /// <summary>Access token lifetime in seconds.</summary>
    [JsonPropertyName("expires_in")]
    public long? ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }
}
