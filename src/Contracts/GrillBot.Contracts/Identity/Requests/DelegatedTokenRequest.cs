namespace GrillBot.Contracts.Identity.Requests;

/// <summary>
/// The identity a trusted client asks IdentityService to issue a token for, through the
/// <see cref="IdentityProtocol.GrantTypes.Delegation"/> grant.
/// </summary>
/// <param name="Subject">Discord user ID, or the API client's ID.</param>
/// <param name="Name">Display name written to the <c>unique_name</c> claim.</param>
/// <param name="Role">Legacy role (<c>Administrator</c>, <c>User</c>, <c>Command</c>, <c>ApiV2</c>, <c>Admin</c>).</param>
/// <param name="Permissions">Permission strings, joined into the <c>GrillBot:Permissions</c> claim.</param>
/// <param name="Channel">One of <see cref="IdentityProtocol.Channels"/>.</param>
/// <param name="Lifetime">Requested access token lifetime; IdentityService caps it.</param>
/// <param name="AvatarUrl">Optional avatar URL.</param>
/// <param name="ThirdPartyKey">API client identifier, for third-party callers only.</param>
/// <param name="Offline">Also issue a refresh token.</param>
public record DelegatedTokenRequest(
    string Subject,
    string Name,
    string? Role,
    IReadOnlyCollection<string> Permissions,
    string Channel,
    TimeSpan Lifetime,
    string? AvatarUrl = null,
    string? ThirdPartyKey = null,
    bool Offline = false
);
