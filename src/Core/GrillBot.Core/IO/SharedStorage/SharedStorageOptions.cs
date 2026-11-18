using System.ComponentModel.DataAnnotations;

namespace GrillBot.Core.IO.SharedStorage;

/// <summary>
/// Directory shared by the services and the bot on one host (a Docker volume mounted into every container
/// that exchanges files). Large payloads are written here and only the file name travels over the broker.
/// </summary>
public sealed class SharedStorageOptions
{
    public const string SectionName = "SharedStorage";

    /// <summary>Absolute path of the shared directory, e.g. <c>/data/shared/archivation</c>.</summary>
    [Required]
    public string Path { get; set; } = "";
}
