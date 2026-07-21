namespace GrillBot.Core.Infrastructure.Auth;

public interface ICurrentUserProvider
{
    string? EncodedJwtToken { get; }
    string? Id { get; }
    bool IsLogged { get; }
    bool IsThirdParty { get; }
    string? Name { get; }
    string[] Permissions { get; }
    string? Role { get; }
    string? ThirdPartyKey { get; }

    void SetCustomToken(string jwtToken);
    Dictionary<string, string>? ToDictionary();
}