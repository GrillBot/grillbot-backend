namespace GrillBot.App.Infrastructure.Options;

/// <summary>
/// The "Auth:Session" section: browser sessions for AdminWeb. The browser holds only an
/// HttpOnly session cookie; the access token lives server-side in Redis and is renewed
/// by the bot before it expires.
/// </summary>
public class WebSessionOptions
{
    public const string SectionName = "Auth:Session";

    /// <summary>
    /// The <c>__Host-</c> prefix pins the cookie to this exact host over HTTPS, so the
    /// legacy admins on other subdomains can neither read nor overwrite it.
    /// </summary>
    public string CookieName { get; set; } = "__Host-grillbot.sid";

    /// <summary>A session unused for this long ends.</summary>
    public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromDays(7);

    /// <summary>A session ends this long after sign-in, used or not.</summary>
    public TimeSpan AbsoluteTimeout { get; set; } = TimeSpan.FromDays(30);

    /// <summary>Lifetime of the access token kept in the session; it is renewed shortly before it expires.</summary>
    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>Where AdminWeb runs; the sign-in flow ends with a redirect there.</summary>
    [Required]
    public string FrontendUrl { get; set; } = "";
}
