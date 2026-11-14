using GrillBot.Contracts.Identity;
using GrillBot.Contracts.Identity.Requests;
using GrillBot.Contracts.Identity.Responses;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Json;

namespace GrillBot.Core.Services.IdentityService;

public class IdentityTokenClient(
    HttpClient _httpClient,
    IOptions<IdentityClientOptions> _options,
    TimeProvider _timeProvider
) : IIdentityTokenClient
{
    public Task<IdentityToken> RequestDelegatedTokenAsync(DelegatedTokenRequest request, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = IdentityProtocol.GrantTypes.Delegation,
            [IdentityProtocol.Parameters.Subject] = request.Subject,
            [IdentityProtocol.Parameters.Name] = request.Name,
            [IdentityProtocol.Parameters.Channel] = request.Channel,
            [IdentityProtocol.Parameters.Lifetime] = ((long)Math.Ceiling(request.Lifetime.TotalSeconds)).ToString(CultureInfo.InvariantCulture)
        };

        AddIfPresent(parameters, IdentityProtocol.Parameters.Role, request.Role);
        AddIfPresent(parameters, IdentityProtocol.Parameters.AvatarUrl, request.AvatarUrl);
        AddIfPresent(parameters, IdentityProtocol.Parameters.ThirdPartyKey, request.ThirdPartyKey);

        if (request.Permissions.Count > 0)
            parameters[IdentityProtocol.Parameters.Permissions] = string.Join(',', request.Permissions);
        if (request.Offline)
            parameters[IdentityProtocol.Parameters.Offline] = "true";

        return SendAsync(parameters, cancellationToken);
    }

    public Task<IdentityToken> RequestClientCredentialsTokenAsync(CancellationToken cancellationToken = default)
        => SendAsync(new Dictionary<string, string> { ["grant_type"] = "client_credentials" }, cancellationToken);

    private async Task<IdentityToken> SendAsync(Dictionary<string, string> parameters, CancellationToken cancellationToken)
    {
        parameters["client_id"] = _options.Value.ClientId;
        parameters["client_secret"] = _options.Value.ClientSecret;

        using var content = new FormUrlEncodedContent(parameters);
        using var response = await _httpClient.PostAsync(IdentityProtocol.TokenEndpoint, content, cancellationToken);

        TokenResponse? body = null;
        if (response.Content.Headers.ContentType?.MediaType == "application/json")
            body = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);

        if (!response.IsSuccessStatusCode || !string.IsNullOrEmpty(body?.Error) || string.IsNullOrEmpty(body?.AccessToken))
            throw new IdentityTokenException(response.StatusCode, body?.Error, body?.ErrorDescription);

        var expiresAt = _timeProvider.GetUtcNow().AddSeconds(body.ExpiresIn ?? 0);
        return new IdentityToken(body.AccessToken, expiresAt, body.RefreshToken);
    }

    private static void AddIfPresent(Dictionary<string, string> parameters, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
            parameters[name] = value;
    }
}
