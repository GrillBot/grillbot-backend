namespace GrillBot.App.Infrastructure.Options;

/// <summary>
/// The start-critical part of the "Discord" section. The rest of the section (logging
/// channel, emotes, message-cache period) has working defaults in appsettings.json and is
/// read straight from <see cref="IConfiguration"/> where it is needed; only the token is
/// declared here, because the bot is a Discord gateway client and has nothing to do without
/// one.
/// </summary>
public class DiscordOptions
{
    public const string SectionName = "Discord";

    [Required(ErrorMessage = "Discord:Token is required - the bot cannot log in to the Discord gateway without it.")]
    public string Token { get; set; } = null!;
}
