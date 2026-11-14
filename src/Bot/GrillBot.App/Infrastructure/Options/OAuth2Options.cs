namespace GrillBot.App.Infrastructure.Options;

/// <summary>
/// The start-critical part of the "Auth:OAuth2" section. The client credentials are read
/// while the host is being built to configure the Discord OAuth handler, so an unfilled
/// placeholder would only surface on the first sign-in. (Tokens are signed by IdentityService.)
///
/// The redirect URLs are not here: they are committed with real values in appsettings.json.
/// </summary>
public class OAuth2Options
{
    public const string SectionName = "Auth:OAuth2";

    [Required(ErrorMessage = "Auth:OAuth2:ClientId is required - it identifies the Discord application used for admin login.")]
    public string ClientId { get; set; } = null!;

    [Required(ErrorMessage = "Auth:OAuth2:ClientSecret is required - the Discord login exchanges its codes with it.")]
    public string ClientSecret { get; set; } = null!;
}
