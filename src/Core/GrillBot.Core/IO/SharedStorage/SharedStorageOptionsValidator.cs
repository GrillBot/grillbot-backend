using Microsoft.Extensions.Options;

namespace GrillBot.Core.IO.SharedStorage;

/// <summary>
/// Fails the startup when the shared directory is not absolute, does not exist or cannot be written, instead of
/// failing on the first archivation run hours later.
/// </summary>
/// <remarks>
/// The directory is deliberately not created. A mounted volume always exists; creating a missing one would
/// give every container its own private directory and turn a missing mount into files that never arrive.
/// </remarks>
public sealed class SharedStorageOptionsValidator : IValidateOptions<SharedStorageOptions>
{
    public ValidateOptionsResult Validate(string? name, SharedStorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Path))
            return ValidateOptionsResult.Fail($"\"{SharedStorageOptions.SectionName}:Path\" is empty.");

        if (!Path.IsPathFullyQualified(options.Path))
            return ValidateOptionsResult.Fail($"\"{SharedStorageOptions.SectionName}:Path\" must be an absolute path, got \"{options.Path}\".");

        if (!Directory.Exists(options.Path))
            return ValidateOptionsResult.Fail($"\"{SharedStorageOptions.SectionName}:Path\" ({options.Path}) does not exist. Is the shared volume mounted?");

        try
        {
            var probe = Path.Combine(options.Path, $".probe_{Guid.NewGuid():N}");
            File.WriteAllBytes(probe, []);
            File.Delete(probe);

            return ValidateOptionsResult.Success;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return ValidateOptionsResult.Fail($"\"{SharedStorageOptions.SectionName}:Path\" ({options.Path}) is not writable: {ex.Message}");
        }
    }
}
