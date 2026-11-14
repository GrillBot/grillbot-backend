using System.ComponentModel.DataAnnotations;

namespace GrillBot.Core.Services.IdentityService;

/// <summary>
/// The <c>Services:IdentityService</c> section: where IdentityService is and the client
/// credentials this host authenticates with. <see cref="ClientSecret"/> comes from /run/secrets.
/// </summary>
public class IdentityClientOptions
{
    public const string SectionName = "Services:IdentityService";

    /// <summary>Base URL of IdentityService on the LAN, also its token issuer and JWKS host.</summary>
    [Required]
    public string Api { get; set; } = "";

    [Required]
    public string ClientId { get; set; } = "";

    [Required]
    public string ClientSecret { get; set; } = "";

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}
