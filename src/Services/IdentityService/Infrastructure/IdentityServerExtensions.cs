using GrillBot.Contracts.Identity;
using IdentityService.Core.Entity;
using IdentityService.Options;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityService.Infrastructure;

public static class IdentityServerExtensions
{
    public static IServiceCollection AddIdentityServer(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(IdentityServerOptions.SectionName);
        var options = section.Get<IdentityServerOptions>() ?? new IdentityServerOptions();
        var isDevelopment = string.Equals(configuration[HostDefaults.EnvironmentKey], Environments.Development, StringComparison.OrdinalIgnoreCase);

        ValidateOptions(options, isDevelopment);

        // The shared telemetry setup logs whole request and response bodies. Here those are the
        // token responses (access and refresh tokens) and the token requests (client secrets),
        // so this service logs only the request line, headers (redacted by default) and status.
        services.Configure<HttpLoggingOptions>(opt => opt.LoggingFields =
            HttpLoggingFields.RequestPropertiesAndHeaders | HttpLoggingFields.ResponsePropertiesAndHeaders | HttpLoggingFields.Duration);

        services.Configure<IdentityServerOptions>(section);
        services.AddScoped<ClientRegistrar>();
        services.AddHostedService<TokenPruningWorker>();

        services.AddOpenIddict()
            .AddCore(builder => builder.UseEntityFrameworkCore().UseDbContext<IdentityContext>())
            .AddServer(builder =>
            {
                builder.SetTokenEndpointUris(IdentityProtocol.TokenEndpoint);

                if (!string.IsNullOrEmpty(options.Issuer))
                    builder.SetIssuer(new Uri(options.Issuer, UriKind.Absolute));

                builder
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow()
                    .AllowCustomFlow(IdentityProtocol.GrantTypes.Delegation);

                builder.RegisterScopes(Scopes.OfflineAccess);

                builder.SetAccessTokenLifetime(options.AccessTokenLifetime);
                builder.SetRefreshTokenLifetime(options.RefreshTokenLifetime);
                builder.SetRefreshTokenReuseLeeway(options.RefreshTokenReuseLeeway);

                // Access tokens stay signed JWTs readable by every service; only refresh tokens are encrypted.
                builder.DisableAccessTokenEncryption();

                AddSigningKeys(builder, options, isDevelopment);
                AddEncryptionKey(builder, options, isDevelopment);

                // IdentityService is reachable only on the LAN, behind the bot; TLS ends at the edge.
                builder.UseAspNetCore()
                    .EnableTokenEndpointPassthrough()
                    .DisableTransportSecurityRequirement();
            });

        return services;
    }

    private static void ValidateOptions(IdentityServerOptions options, bool isDevelopment)
    {
        if (options.MaxDelegatedTokenLifetime <= TimeSpan.Zero || options.AccessTokenLifetime <= TimeSpan.Zero)
            throw new InvalidOperationException("Identity: token lifetimes must be positive.");

        if (isDevelopment)
            return;

        var missing = new List<string>();
        if (string.IsNullOrEmpty(options.Issuer))
            missing.Add("Identity:Issuer");
        if (options.SigningKeys.Count == 0)
            missing.Add("Identity:SigningKeys");
        if (string.IsNullOrEmpty(options.EncryptionKey))
            missing.Add("Identity:EncryptionKey");

        missing.AddRange(
            options.Clients
                .Where(o => string.IsNullOrEmpty(o.Value.Secret))
                .Select(o => $"Identity:Clients:{o.Key}:Secret")
        );

        if (missing.Count > 0)
            throw new InvalidOperationException($"IdentityService is missing required configuration: {string.Join(", ", missing)}. Provide them via /run/secrets.");
    }

    private static void AddSigningKeys(OpenIddictServerBuilder builder, IdentityServerOptions options, bool isDevelopment)
    {
        if (options.SigningKeys.Count == 0 && isDevelopment)
        {
            // Development only: a new key on every start, so tokens do not survive a restart.
            builder.AddEphemeralSigningKey(SecurityAlgorithms.EcdsaSha256);
            return;
        }

        foreach (var key in options.SigningKeys)
        {
            if (string.IsNullOrEmpty(key.KeyId))
                throw new InvalidOperationException("Identity:SigningKeys: every key needs a KeyId.");

            var ecdsa = ECDsa.Create();
            ecdsa.ImportFromPem(key.Pem);

            if (ecdsa.KeySize != 256)
                throw new InvalidOperationException($"Identity:SigningKeys: key '{key.KeyId}' must be a P-256 key (ES256).");

            builder.AddSigningKey(new ECDsaSecurityKey(ecdsa) { KeyId = key.KeyId });
        }
    }

    private static void AddEncryptionKey(OpenIddictServerBuilder builder, IdentityServerOptions options, bool isDevelopment)
    {
        if (string.IsNullOrEmpty(options.EncryptionKey) && isDevelopment)
        {
            builder.AddEphemeralEncryptionKey();
            return;
        }

        var key = Convert.FromBase64String(options.EncryptionKey!);
        if (key.Length != 32)
            throw new InvalidOperationException("Identity:EncryptionKey must be base64 of exactly 32 bytes.");

        builder.AddEncryptionKey(new SymmetricSecurityKey(key));
    }
}
