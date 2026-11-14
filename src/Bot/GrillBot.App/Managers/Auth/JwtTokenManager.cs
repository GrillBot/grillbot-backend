using GrillBot.App.Managers.Points;
using GrillBot.Common.Managers.Localization;
using GrillBot.Contracts.Identity;
using GrillBot.Contracts.Identity.Requests;
using GrillBot.Core.Services.IdentityService;
using GrillBot.Data.Models.API.OAuth2;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GrillBot.App.Managers.Auth;

/// <summary>
/// Decides who gets which role and permissions, then has IdentityService issue the token.
/// The bot no longer signs tokens itself; until IdentityService owns the permission model,
/// it passes the claims it computed here through the delegation grant.
/// </summary>
public class JwtTokenManager(
    GrillBotDatabaseBuilder _databaseBuilder,
    ITextsManager _texts,
    IServiceProvider _serviceProvider,
    IWebHostEnvironment _environment,
    IIdentityTokenClient _identityClient
)
{
    // channel = where the request comes from; ignored (always Discord) when an interaction is passed.
    // lifetime = overrides the role-based token lifetime (browser sessions use short, renewed tokens).
    public async Task<OAuth2LoginToken> CreateTokenForUserAsync(IUser user, string language, IInteractionContext? interaction = null, string channel = IdentityProtocol.Channels.Web,
        TimeSpan? lifetime = null)
    {
        using var repository = _databaseBuilder.CreateRepository();

        var userEntity = await repository.User.FindUserAsync(user, true);
        if (userEntity is null)
            return new(_texts["Auth/CreateToken/UserNotFound", language]);

        var userRole = ResolveRole(userEntity, interaction);
        if (string.IsNullOrEmpty(userRole))
            return new OAuth2LoginToken(string.Format(_texts["Auth/CreateToken/PublicAdminBlocked", language], user.Username));

        await SynchronizeUserToServicesAsync(user);

        var resolvedChannel = interaction is null ? channel : IdentityProtocol.Channels.Discord;
        return await IssueTokenAsync(userEntity, userRole, interaction, null, resolvedChannel, lifetime);
    }

    /// <summary>
    /// Issues a fresh web token for an existing browser session. Role and permissions are computed
    /// again, so a user who lost web access (or the account) is refused here.
    /// </summary>
    public async Task<OAuth2LoginToken> RenewTokenForUserAsync(ulong userId, string language, TimeSpan lifetime)
    {
        using var repository = _databaseBuilder.CreateRepository();

        var userEntity = await repository.User.FindUserByIdAsync(userId, disableTracking: true);
        if (userEntity is null)
            return new(_texts["Auth/CreateToken/UserNotFound", language]);

        var userRole = ResolveRole(userEntity, null);
        if (string.IsNullOrEmpty(userRole))
            return new OAuth2LoginToken(string.Format(_texts["Auth/CreateToken/PublicAdminBlocked", language], userEntity.Username));

        return await IssueTokenAsync(userEntity, userRole, null, null, IdentityProtocol.Channels.Web, lifetime);
    }

    public Task<OAuth2LoginToken> CreateTokenForApiClientAsync(ApiClient client, string language)
    {
        var userAvatarId = SnowflakeUtils.ToSnowflake(DateTimeOffset.UtcNow);
        var userEntity = new User
        {
            GlobalAlias = client.Name,
            Language = language,
            Id = client.Id,
            Status = UserStatus.Online,
            Username = client.Name,
            AvatarUrl = CDN.GetDefaultUserAvatarUrl(userAvatarId)
        };

        return IssueTokenAsync(userEntity, "ApiV2", null, client, IdentityProtocol.Channels.Api);
    }

    private static string? ResolveRole(User user, IInteractionContext? interaction)
    {
        if (interaction is not null)
            return user.HaveFlags(UserFlags.CommandsDisabled) ? null : "Command";

        if (user.HaveFlags(UserFlags.WebAdminDisabled))
            return null;
        return user.HaveFlags(UserFlags.WebAdmin) ? "Administrator" : "User";
    }

    private async Task<OAuth2LoginToken> IssueTokenAsync(User user, string userRole, IInteractionContext? interaction, ApiClient? apiClient, string channel, TimeSpan? lifetime = null)
    {
        var request = new DelegatedTokenRequest(
            Subject: user.Id,
            Name: user.Username,
            Role: userRole,
            Permissions: [.. CreatePermissions(userRole, interaction, apiClient)],
            Channel: channel,
            Lifetime: lifetime ?? ResolveTokenLifetime(userRole),
            AvatarUrl: string.IsNullOrEmpty(user.AvatarUrl) ? null : user.AvatarUrl,
            ThirdPartyKey: apiClient?.Id
        );

        var token = await _identityClient.RequestDelegatedTokenAsync(request);
        return new OAuth2LoginToken(token.AccessToken, token.ExpiresAt);
    }

    private TimeSpan ResolveTokenLifetime(string userRole)
    {
        // IdentityService caps every lifetime at Identity:MaxDelegatedTokenLifetime (1 day by default).
        if (_environment.IsDevelopment())
            return TimeSpan.FromDays(1);

        return userRole switch
        {
            "Administrator" => TimeSpan.FromDays(1),
            "Command" => TimeSpan.FromMinutes(20),
            "ApiV2" => TimeSpan.FromMinutes(10),
            "User" => TimeSpan.FromHours(4),
            _ => throw new NotSupportedException()
        };
    }

    private static IEnumerable<string> CreatePermissions(string role, IInteractionContext? interaction, ApiClient? apiClient)
    {
        if (interaction is not null)
        {
            yield return interaction.Interaction.Data?.ToString() ?? "-";
            yield return $"GuildId:{interaction.Guild?.Id}";
            yield break;
        }

        if (apiClient is not null)
        {
            foreach (var method in apiClient.AllowedMethods)
                yield return method;
            yield break;
        }

        if (role == "Administrator")
        {
            yield return "Dashboard(Admin)";
            yield return "AuditLog(Admin)";
            yield return "Emote(Admin)";
            yield return "Points(Admin)";
            yield return "Remind(Admin)";
            yield return "Infractions(Admin)";
            yield return "Searching(Admin)";
            yield return "Invite(Admin)";
            yield return "Message(Admin)";
            yield return "Unverify(Admin)";
        }
        else
        {
            yield return "Remind(OnlyMyReminders)";
            yield return "Remind(CancelMyReminders)";
            yield return "Infractions(OnlyMine)";
            yield return "Searching(OnlyMySearches)";
        }

        yield return "FrontendLog";
        yield return "Lookups";
        yield return "Points(Leaderboard)";
        yield return "Points(UserStatus)";
        yield return "Filters";
    }

    private async Task SynchronizeUserToServicesAsync(IUser user)
    {
        await _serviceProvider.GetRequiredService<PointsManager>()
            .PushSynchronizationUsersAsync(new[] { user });
    }
}
