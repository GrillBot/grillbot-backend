using GrillBot.Contracts.Identity;
using GrillBot.Contracts.Identity.Requests;
using GrillBot.Core.Services.IdentityService;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace GrillBot.Core.Services.Tests.IdentityService;

[TestClass]
public class IdentityTokenClientTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);

    private RecordingHandler _handler = null!;
    private IdentityTokenClient _client = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _handler = new RecordingHandler();

        var httpClient = new HttpClient(_handler) { BaseAddress = new Uri("http://identity.test/") };
        var options = Options.Create(new IdentityClientOptions { Api = "http://identity.test/", ClientId = "bot", ClientSecret = "secret" });

        _client = new IdentityTokenClient(httpClient, options, new FixedTimeProvider(Now));
    }

    [TestMethod]
    public async Task RequestDelegatedToken_SendsGrantAndSubjectClaims()
    {
        _handler.Respond(HttpStatusCode.OK, """{"access_token":"jwt","token_type":"Bearer","expires_in":600}""");

        var request = new DelegatedTokenRequest(
            Subject: "123",
            Name: "user",
            Role: "Administrator",
            Permissions: ["Points(Admin)", "Lookups"],
            Channel: IdentityProtocol.Channels.Web,
            Lifetime: TimeSpan.FromHours(4),
            AvatarUrl: "https://cdn/avatar.png"
        );

        var token = await _client.RequestDelegatedTokenAsync(request);

        Assert.AreEqual("jwt", token.AccessToken);
        Assert.AreEqual(Now.AddSeconds(600), token.ExpiresAt);
        Assert.IsNull(token.RefreshToken);

        Assert.AreEqual(HttpMethod.Post, _handler.Method);
        Assert.AreEqual("http://identity.test/connect/token", _handler.Uri?.ToString());

        var form = _handler.Form;
        Assert.AreEqual(IdentityProtocol.GrantTypes.Delegation, form["grant_type"]);
        Assert.AreEqual("bot", form["client_id"]);
        Assert.AreEqual("secret", form["client_secret"]);
        Assert.AreEqual("123", form[IdentityProtocol.Parameters.Subject]);
        Assert.AreEqual("user", form[IdentityProtocol.Parameters.Name]);
        Assert.AreEqual("Administrator", form[IdentityProtocol.Parameters.Role]);
        Assert.AreEqual("Points(Admin),Lookups", form[IdentityProtocol.Parameters.Permissions]);
        Assert.AreEqual("web", form[IdentityProtocol.Parameters.Channel]);
        Assert.AreEqual("14400", form[IdentityProtocol.Parameters.Lifetime]);
        Assert.AreEqual("https://cdn/avatar.png", form[IdentityProtocol.Parameters.AvatarUrl]);
        Assert.IsFalse(form.ContainsKey(IdentityProtocol.Parameters.ThirdPartyKey));
        Assert.IsFalse(form.ContainsKey(IdentityProtocol.Parameters.Offline));
    }

    [TestMethod]
    public async Task RequestDelegatedToken_OfflineAndEmptyPermissions()
    {
        _handler.Respond(HttpStatusCode.OK, """{"access_token":"jwt","expires_in":60,"refresh_token":"refresh"}""");

        var request = new DelegatedTokenRequest("1", "client", "ApiV2", [], IdentityProtocol.Channels.Api, TimeSpan.FromSeconds(0.5), ThirdPartyKey: "key", Offline: true);
        var token = await _client.RequestDelegatedTokenAsync(request);

        Assert.AreEqual("refresh", token.RefreshToken);
        Assert.AreEqual("true", _handler.Form[IdentityProtocol.Parameters.Offline]);
        Assert.AreEqual("1", _handler.Form[IdentityProtocol.Parameters.Lifetime]);
        Assert.AreEqual("key", _handler.Form[IdentityProtocol.Parameters.ThirdPartyKey]);
        Assert.IsFalse(_handler.Form.ContainsKey(IdentityProtocol.Parameters.Permissions));
    }

    [TestMethod]
    public async Task RequestClientCredentialsToken_SendsOnlyClientCredentials()
    {
        _handler.Respond(HttpStatusCode.OK, """{"access_token":"jwt","expires_in":3600}""");

        await _client.RequestClientCredentialsTokenAsync();

        CollectionAssert.AreEquivalent(new[] { "grant_type", "client_id", "client_secret" }, _handler.Form.Keys.ToArray());
        Assert.AreEqual("client_credentials", _handler.Form["grant_type"]);
    }

    [TestMethod]
    public async Task RequestToken_ErrorResponse_Throws()
    {
        _handler.Respond(HttpStatusCode.BadRequest, """{"error":"invalid_request","error_description":"The subject parameter is required."}""");

        var ex = await Assert.ThrowsExactlyAsync<IdentityTokenException>(() => _client.RequestClientCredentialsTokenAsync());

        Assert.AreEqual(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.AreEqual("invalid_request", ex.Error);
        Assert.AreEqual("The subject parameter is required.", ex.ErrorDescription);
    }

    [TestMethod]
    public async Task RequestToken_NonJsonFailure_Throws()
    {
        _handler.Respond(HttpStatusCode.BadGateway, "<html>bad gateway</html>", "text/html");

        var ex = await Assert.ThrowsExactlyAsync<IdentityTokenException>(() => _client.RequestClientCredentialsTokenAsync());

        Assert.AreEqual(HttpStatusCode.BadGateway, ex.StatusCode);
        Assert.IsNull(ex.Error);
    }

    [TestMethod]
    public async Task RequestToken_SuccessWithoutAccessToken_Throws()
    {
        _handler.Respond(HttpStatusCode.OK, """{"token_type":"Bearer"}""");

        await Assert.ThrowsExactlyAsync<IdentityTokenException>(() => _client.RequestClientCredentialsTokenAsync());
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private HttpStatusCode _statusCode;
        private string _body = "";
        private string _mediaType = "application/json";

        public HttpMethod? Method { get; private set; }
        public Uri? Uri { get; private set; }
        public Dictionary<string, string> Form { get; } = [];

        public void Respond(HttpStatusCode statusCode, string body, string mediaType = "application/json")
        {
            _statusCode = statusCode;
            _body = body;
            _mediaType = mediaType;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Method = request.Method;
            Uri = request.RequestUri;

            var content = await request.Content!.ReadAsStringAsync(cancellationToken);
            foreach (var pair in content.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = pair.Split('=', 2);
                Form[WebUtility.UrlDecode(parts[0])] = WebUtility.UrlDecode(parts[1]);
            }

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, Encoding.UTF8, _mediaType)
            };
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
