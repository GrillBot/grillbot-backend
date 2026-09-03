namespace GrillBot.App.Infrastructure.Options;

/// <summary>
/// The start-critical part of the "Auth:OAuth2" section. The client credentials are read
/// while the host is being built - they configure the Discord OAuth handler and they are the
/// material the API's JWT signing key is derived from - so an unfilled placeholder would
/// leave the bot serving an API whose every token is signed with "_".
///
/// The redirect URLs are not here: they are committed with real values in appsettings.json.
/// </summary>
public class OAuth2Options
{
    public const string SectionName = "Auth:OAuth2";

    [Required(ErrorMessage = "Auth:OAuth2:ClientId is required - it identifies the Discord application used for admin login and JWT signing.")]
    public string ClientId { get; set; } = null!;

    [Required(ErrorMessage = "Auth:OAuth2:ClientSecret is required - it is part of the API's JWT signing key.")]
    public string ClientSecret { get; set; } = null!;
}
