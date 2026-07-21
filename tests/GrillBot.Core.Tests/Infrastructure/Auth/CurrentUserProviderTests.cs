using GrillBot.Core.Infrastructure.Auth;

namespace GrillBot.Core.Tests.Infrastructure.Auth;

[TestClass]
public class CurrentUserProviderTests
{
    // Generated for testing purposes. This time is expired.
    private const string TEST_JWT_TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImhvYml0IiwibmFtZWlkIjoiMzcwNTA2ODIwMTk3ODEwMTc2Iiwicm9sZSI6IkFkbWluaXN0cmF0b3IiLCJuYmYiOjE3MTk1NTQ2NjgsImV4cCI6MTcyMjE0NjY2OCwiaWF0IjoxNzE5NTU0NjY4LCJpc3MiOiJHcmlsbEJvdC9ERVNLVE9QLU5IOEdQNE8vbWhhbGEiLCJhdWQiOiJHcmlsbEJvdC9ERVNLVE9QLU5IOEdQNE8vbWhhbGEifQ.qg8m-aXNlugo45-V2v64Ez8VjW8oGvkaaSDKK2UrvX8";

    [TestMethod]
    public void MissingToken_NotLoggedIn()
    {
        var headers = new Dictionary<string, string>();
        var provider = new CurrentUserProvider(headers);

        Assert.IsNull(provider.EncodedJwtToken);
        Assert.IsFalse(provider.IsLogged);
    }

    [TestMethod]
    public void WithToken_LoggedIn()
    {
        var headers = new Dictionary<string, string>();
        var provider = new CurrentUserProvider(headers);

        provider.SetCustomToken(TEST_JWT_TOKEN);

        Assert.IsNotNull(provider.EncodedJwtToken);
        Assert.IsTrue(provider.IsLogged);
        Assert.IsFalse(provider.IsThirdParty);
        Assert.IsNotNull(provider.Name);
        Assert.IsNotNull(provider.Id);
        Assert.IsNotNull(provider.Role);
        Assert.IsNull(provider.ThirdPartyKey);
    }

    [TestMethod]
    public void WithToken_InvalidToken()
    {
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"ApiKey {Guid.NewGuid()}" }
        };

        var provider = new CurrentUserProvider(headers);

        Assert.IsNull(provider.EncodedJwtToken);
        Assert.IsFalse(provider.IsLogged);
        Assert.IsFalse(provider.IsThirdParty);
        Assert.IsNull(provider.Name);
    }
}
