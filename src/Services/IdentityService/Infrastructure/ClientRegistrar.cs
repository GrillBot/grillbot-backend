using IdentityService.Options;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityService.Infrastructure;

/// <summary>
/// Makes the client registry in the database match <see cref="IdentityServerOptions.Clients"/>:
/// creates missing clients, updates secrets and grant types, deletes clients no longer configured.
/// </summary>
public class ClientRegistrar(
    IOpenIddictApplicationManager _applications,
    IOptions<IdentityServerOptions> _options,
    ILogger<ClientRegistrar> _logger
)
{
    public async Task SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        var configured = _options.Value.Clients;

        foreach (var (clientId, client) in configured)
        {
            if (string.IsNullOrEmpty(client.Secret))
            {
                _logger.LogWarning("Client {ClientId} has no secret configured and was not registered.", clientId);
                continue;
            }

            var descriptor = CreateDescriptor(clientId, client);
            var application = await _applications.FindByClientIdAsync(clientId, cancellationToken);

            if (application is null)
                await _applications.CreateAsync(descriptor, cancellationToken);
            else
                await _applications.UpdateAsync(application, descriptor, cancellationToken);
        }

        var stale = new List<object>();
        await foreach (var application in _applications.ListAsync(null, null, cancellationToken))
        {
            var clientId = await _applications.GetClientIdAsync(application, cancellationToken);
            if (clientId is null || !configured.ContainsKey(clientId))
                stale.Add(application);
        }

        foreach (var application in stale)
            await _applications.DeleteAsync(application, cancellationToken);

        _logger.LogInformation("Client registry synchronized: {Count} configured, {Removed} removed.", configured.Count, stale.Count);
    }

    private static OpenIddictApplicationDescriptor CreateDescriptor(string clientId, ClientOptions client)
    {
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = client.Secret,
            ClientType = ClientTypes.Confidential,
            DisplayName = client.DisplayName ?? clientId
        };

        descriptor.Permissions.Add(Permissions.Endpoints.Token);
        foreach (var grantType in client.GrantTypes)
            descriptor.Permissions.Add(Permissions.Prefixes.GrantType + grantType);

        return descriptor;
    }
}
