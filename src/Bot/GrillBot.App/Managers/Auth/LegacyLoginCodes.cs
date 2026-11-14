using Microsoft.Extensions.Caching.Distributed;
using System.Buffers.Text;
using System.Security.Cryptography;

namespace GrillBot.App.Managers.Auth;

/// <summary>
/// One-time codes for the legacy admins' V1 login. The OAuth callback used to put the user's
/// Discord access token into the redirect URL (<c>?sessionId=...</c>), where it ended up in
/// browser history and access logs. Now the URL carries a random code valid for one minute,
/// which <c>api/auth/token</c> redeems once. The legacy frontends pass the value through
/// without reading it, so they need no change.
/// </summary>
public class LegacyLoginCodes(IDistributedCache _cache)
{
    private const string KeyPrefix = "auth:legacy-code:";
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(1);

    public async Task<string> IssueAsync(string discordAccessToken)
    {
        var code = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Lifetime };

        await _cache.SetStringAsync(CreateKey(code), discordAccessToken, options);
        return code;
    }

    /// <returns>The Discord access token, or <c>null</c> when the code is unknown, expired or already used.</returns>
    public async Task<string?> RedeemAsync(string code)
    {
        var key = CreateKey(code);
        var token = await _cache.GetStringAsync(key);

        if (token is not null)
            await _cache.RemoveAsync(key);
        return token;
    }

    private static string CreateKey(string code)
        => KeyPrefix + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));
}
