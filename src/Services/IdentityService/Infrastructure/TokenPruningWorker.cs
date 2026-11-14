using IdentityService.Options;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;

namespace IdentityService.Infrastructure;

/// <summary>
/// Every token issued is stored as an entry (that is what makes refresh tokens revocable),
/// and slash commands request one per call. This deletes the expired and invalid entries.
/// </summary>
public class TokenPruningWorker(
    IServiceScopeFactory _scopeFactory,
    IOptions<IdentityServerOptions> _options,
    ILogger<TokenPruningWorker> _logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.Value.TokenPruneInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await PruneAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Token pruning failed.");
            }
        }
    }

    private async Task PruneAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var threshold = DateTimeOffset.UtcNow - _options.Value.TokenPruneThreshold;

        var tokens = await scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>().PruneAsync(threshold, cancellationToken);
        var authorizations = await scope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>().PruneAsync(threshold, cancellationToken);

        _logger.LogInformation("Pruned {Tokens} tokens and {Authorizations} authorizations.", tokens, authorizations);
    }
}
