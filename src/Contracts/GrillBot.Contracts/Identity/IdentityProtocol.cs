namespace GrillBot.Contracts.Identity;

/// <summary>
/// Names shared by IdentityService and its callers on the OAuth 2.0 token endpoint:
/// the custom grant type, its request parameters and the claim types the issued
/// tokens carry.
/// </summary>
/// <remarks>
/// The claim names deliberately match what the bot used to write itself
/// (<c>nameid</c>, <c>unique_name</c>, <c>role</c>, <c>GrillBot:Permissions</c>, ...),
/// so AdminWeb, the legacy frontends and <c>CurrentUserProvider</c> read tokens
/// issued by IdentityService without a change.
/// </remarks>
public static class IdentityProtocol
{
    /// <summary>Relative path of the token endpoint.</summary>
    public const string TokenEndpoint = "connect/token";

    /// <summary>Audience every access token is issued for.</summary>
    public const string DefaultAudience = "grillbot";

    public static class GrantTypes
    {
        /// <summary>
        /// A trusted client (the bot) asks for a token on behalf of a subject and supplies
        /// the subject's claims itself. Temporary until IdentityService owns the
        /// permission model; only clients granted this grant type may use it.
        /// </summary>
        public const string Delegation = "urn:grillbot:grant-type:delegation";
    }

    /// <summary>Request parameters of the <see cref="GrantTypes.Delegation"/> grant.</summary>
    public static class Parameters
    {
        public const string Subject = "subject";
        public const string Name = "subject_name";
        public const string Role = "subject_role";
        public const string Permissions = "subject_permissions";
        public const string AvatarUrl = "subject_avatar_url";
        public const string ThirdPartyKey = "third_party_key";
        public const string Channel = "channel";
        public const string Lifetime = "lifetime";
        public const string Offline = "offline";
    }

    public static class ClaimTypes
    {
        public const string Subject = "sub";
        public const string NameIdentifier = "nameid";
        public const string Name = "unique_name";
        public const string Role = "role";
        public const string Permissions = "GrillBot:Permissions";
        public const string ThirdPartyKey = "GrillBot:ThirdPartyKey";
        public const string AvatarUrl = "http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata";
        public const string Channel = "channel";
        public const string ClientId = "client_id";
    }

    /// <summary>Values of <see cref="ClaimTypes.Channel"/>: where the caller came from.</summary>
    public static class Channels
    {
        public const string Web = "web";
        public const string Discord = "discord";
        public const string Api = "api";
        public const string Job = "job";
        public const string Legacy = "legacy";
        public const string Service = "service";
    }
}
