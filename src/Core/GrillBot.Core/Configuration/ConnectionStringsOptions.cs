using System.ComponentModel.DataAnnotations;

namespace GrillBot.Core.Configuration;

/// <summary>
/// The "ConnectionStrings" section, validated at startup for every host that declares it.
/// </summary>
public class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";

    /// <summary>
    /// The application's own PostgreSQL database. Every host but ImageProcessingService has
    /// one, and each of them runs EF Core migrations before it serves the first request.
    /// </summary>
    [Required(ErrorMessage = "ConnectionStrings:Default is required - it is the application's own PostgreSQL database.")]
    public string Default { get; set; } = null!;

    /// <summary>
    /// Optional Discord bot token for the services that read from Discord (AuditLogService,
    /// RubbergodService). Leaving it empty deliberately turns the Discord client off rather
    /// than failing - see <c>DiscordExtensions.AddDiscord</c>.
    /// </summary>
    public string? BotToken { get; set; }
}
