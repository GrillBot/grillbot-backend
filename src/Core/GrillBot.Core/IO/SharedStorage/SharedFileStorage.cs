using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace GrillBot.Core.IO.SharedStorage;

/// <summary>
/// Files exchanged between the services and the bot through the shared directory (claim check): the producer
/// writes the file and publishes its name, the consumer opens it by that name and deletes it when done.
/// </summary>
/// <remarks>
/// Names come from messages, so every method that takes one accepts only a plain file name that resolves
/// inside the shared directory. Files are written under a <c>.partial</c> name and renamed when complete,
/// so a consumer never sees a half-written file.
/// </remarks>
public sealed partial class SharedFileStorage(string rootPath, TimeProvider timeProvider)
{
    public const string PartialExtension = ".partial";

    /// <summary>Full path of the shared directory.</summary>
    public string RootPath { get; } = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));

    public SharedFileStorage(IOptions<SharedStorageOptions> options)
        : this(options.Value.Path, TimeProvider.System)
    {
    }

    /// <summary>
    /// Starts a new file named <c>{prefix}_{utc:yyyyMMdd_HHmmss}_{random}{extension}</c>. Write to
    /// <see cref="SharedFileWriter.Stream"/> and call <see cref="SharedFileWriter.CommitAsync"/>; disposing an
    /// uncommitted writer deletes the partial file.
    /// </summary>
    public SharedFileWriter Create(string prefix, string extension)
    {
        if (!PrefixRegex().IsMatch(prefix))
            throw new ArgumentException($"Prefix \"{prefix}\" may contain only letters, digits, '_' and '-'.", nameof(prefix));
        if (!ExtensionRegex().IsMatch(extension))
            throw new ArgumentException($"Extension \"{extension}\" must look like \".json.br\".", nameof(extension));

        var name = $"{prefix}_{timeProvider.GetUtcNow():yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{extension}";
        return new SharedFileWriter(name, ResolvePath(name));
    }

    public bool Exists(string name)
        => File.Exists(ResolvePath(name));

    public Stream OpenRead(string name)
        => new FileStream(ResolvePath(name), FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);

    /// <summary>Deletes the file. A file that does not exist is not an error.</summary>
    public void Delete(string name)
        => File.Delete(ResolvePath(name));

    /// <summary>
    /// Deletes files of <paramref name="prefix"/> (finished and partial) last written more than <paramref name="age"/> ago:
    /// leftovers of crashed producers and of messages that were never consumed.
    /// </summary>
    /// <returns>How many files were deleted.</returns>
    public int DeleteExpired(string prefix, TimeSpan age)
    {
        if (!PrefixRegex().IsMatch(prefix))
            throw new ArgumentException($"Prefix \"{prefix}\" may contain only letters, digits, '_' and '-'.", nameof(prefix));
        if (!Directory.Exists(RootPath))
            return 0;

        var threshold = timeProvider.GetUtcNow().UtcDateTime - age;
        var deleted = 0;

        foreach (var file in Directory.EnumerateFiles(RootPath, $"{prefix}_*"))
        {
            if (File.GetLastWriteTimeUtc(file) > threshold)
                continue;

            try
            {
                File.Delete(file);
                deleted++;
            }
            catch (IOException)
            {
                // Still open by someone; the next sweep gets it.
            }
        }

        return deleted;
    }

    /// <summary>Full path of <paramref name="name"/>, after checking it is a plain file name inside the shared directory.</summary>
    private string ResolvePath(string name)
    {
        if (string.IsNullOrWhiteSpace(name)
            || name is "." or ".."
            || name.Contains('/') || name.Contains('\\')
            || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || name.EndsWith(PartialExtension, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(Path.GetFileName(name), name, StringComparison.Ordinal))
        {
            throw new ArgumentException($"\"{name}\" is not a valid shared file name.", nameof(name));
        }

        var path = Path.GetFullPath(Path.Combine(RootPath, name));
        if (!string.Equals(Path.GetDirectoryName(path), RootPath, StringComparison.Ordinal))
            throw new ArgumentException($"\"{name}\" resolves outside of the shared directory.", nameof(name));

        return path;
    }

    [GeneratedRegex("^[A-Za-z0-9_-]+$")]
    private static partial Regex PrefixRegex();

    [GeneratedRegex(@"^(\.[A-Za-z0-9]+)+$")]
    private static partial Regex ExtensionRegex();
}
