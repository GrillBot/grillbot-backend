using System.Net.Http;
using System.Net.Http.Headers;
using GrillBot.App.Managers.Auth;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Contracts.Identity;
using GrillBot.Contracts.Identity.Requests;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.IdentityService;
using GrillBot.Data.Models.API.OAuth2;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Auth;

public class CreateToken : ApiAction
{
    private HttpClient HttpClient { get; }
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private LegacyLoginCodes LoginCodes { get; }
    private IIdentityTokenClient IdentityClient { get; }
    private IdentityAuditLog AuditLog { get; }

    public CreateToken(ApiRequestContext apiContext, IHttpClientFactory httpClientFactory, IDiscordClient discordClient, ITextsManager texts, GrillBotDatabaseBuilder databaseBuilder,
        IIdentityTokenClient identityClient, LegacyLoginCodes loginCodes, IdentityAuditLog auditLog) : base(apiContext)
    {
        HttpClient = httpClientFactory.CreateClient();
        DiscordClient = discordClient;
        Texts = texts;
        DatabaseBuilder = databaseBuilder;
        LoginCodes = loginCodes;
        IdentityClient = identityClient;
        AuditLog = auditLog;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var isPublic = (bool)Parameters[1]!;

        if (Parameters[0] is string sessionId)
        {
            var userId = await GetUserIdAsync(sessionId);
            var token = await ProcessAsync(userId, isPublic);
            return ApiResult.Ok(token);
        }
        else if (Parameters[0] is ulong userId)
        {
            var token = await ProcessAsync(userId, isPublic);
            return ApiResult.Ok(token);
        }

        return ApiResult.BadRequest();
    }

    private async Task<OAuth2LoginToken> ProcessAsync(ulong? userId, bool isPublic)
    {
        var user = await FindUserAsync(userId);

        if (user is null)
            return await RefuseAsync(userId, Texts["Auth/CreateToken/UserNotFound", ApiContext.Language]);

        using var repository = DatabaseBuilder.CreateRepository();

        var userEntity = await repository.User.FindUserAsync(user, true);
        if (userEntity is null)
            return await RefuseAsync(user.Id, Texts["Auth/CreateToken/UserNotFound", ApiContext.Language]);

        var checkResult = CheckUserLogin(userEntity, isPublic);
        if (!string.IsNullOrEmpty(checkResult))
            return await RefuseAsync(user.Id, checkResult);

        var (token, expiresAt) = await CreateJwtTokenAsync(userEntity, isPublic);
        await AuditLog.WriteAsync(IdentityEventType.SignIn, user.Id, IdentityProtocol.Channels.Legacy);
        return new OAuth2LoginToken(token, expiresAt);
    }

    private async Task<OAuth2LoginToken> RefuseAsync(ulong? userId, string reason)
    {
        await AuditLog.WriteAsync(IdentityEventType.SignInRefused, userId, IdentityProtocol.Channels.Legacy, reason);
        return new OAuth2LoginToken(reason);
    }

    private async Task<ulong?> GetUserIdAsync(string sessionId)
    {
        // sessionId is the one-time code from ProcessCallback; it stands for the Discord access token.
        var discordAccessToken = await LoginCodes.RedeemAsync(sessionId);
        if (discordAccessToken is null)
            return null;

        using var message = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", discordAccessToken);

        using var response = await HttpClient.SendAsync(message);
        var json = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            return JObject.Parse(json)["id"]!.Value<ulong>();
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return null;

        throw new WebException(json);
    }

    private async Task<IUser?> FindUserAsync(ulong? userId)
        => userId == null ? null : await DiscordClient.FindUserAsync(userId.Value);

    private string? CheckUserLogin(Database.Entity.User user, bool isPublic)
    {
        return isPublic switch
        {
            true when user.HaveFlags(UserFlags.PublicAdministrationBlocked) => string.Format(Texts["Auth/CreateToken/PublicAdminBlocked", ApiContext.Language], user.Username),
            false when !user.HaveFlags(UserFlags.WebAdmin) => string.Format(Texts["Auth/CreateToken/PrivateAdminDisabled", ApiContext.Language], user.Username),
            _ => null
        };
    }

    private async Task<(string token, DateTimeOffset expiresAt)> CreateJwtTokenAsync(Database.Entity.User user, bool isPublic)
    {
        // Same claims as the V1 token always had (name, nameid, role); IdentityService signs it.
        var request = new DelegatedTokenRequest(
            Subject: user.Id,
            Name: user.Username,
            Role: isPublic ? "User" : "Admin",
            Permissions: [],
            Channel: IdentityProtocol.Channels.Legacy,
            Lifetime: TimeSpan.FromHours(3)
        );

        var token = await IdentityClient.RequestDelegatedTokenAsync(request);
        return (token.AccessToken, token.ExpiresAt);
    }
}
