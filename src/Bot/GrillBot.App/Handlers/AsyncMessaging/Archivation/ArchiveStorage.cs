using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using GrillBot.Common.FileStorage;
using GrillBot.Core.IO;

namespace GrillBot.App.Handlers.AsyncMessaging.Archivation;

/// <summary>
/// Builds an archive ZIP in a temporary file and uploads it. Layout:
/// <list type="bullet">
/// <item><c>data.json</c> - the records exactly as the service wrote them, streamed in without being parsed;</item>
/// <item><c>lookups.json</c> - guilds, channels and users the records reference (<see cref="ArchiveEnricher"/>);</item>
/// <item>further entries from the callback (audit log attachments).</item>
/// </list>
/// Archives made before this layout hold one <c>{name}.json</c> with the lookups merged into the data.
/// </summary>
public static class ArchiveStorage
{
    public const string DataEntryName = "data.json";
    public const string LookupsEntryName = "lookups.json";

    public sealed record Result(string ArchiveName, long JsonSize, long ArchiveSize);

    /// <summary>Name of the uploaded archive for <paramref name="baseName"/>.</summary>
    public static string GetArchiveName(string baseName) => $"{baseName}.zip";

    /// <summary>
    /// Writes the entries into a temporary ZIP, uploads it to <paramref name="target"/> under
    /// <see cref="GetArchiveName"/>, replacing an earlier upload of the same name, and deletes the temporary file.
    /// </summary>
    /// <returns>The uploaded archive name, the uncompressed size of both JSON entries and the ZIP size, in bytes.</returns>
    /// <param name="target">Blob container the archive is uploaded to.</param>
    /// <param name="baseName">Name of the archive without extension. Must be deterministic per archive, so a retried upload replaces the first one.</param>
    /// <param name="data">Uncompressed JSON of the records, copied into <see cref="DataEntryName"/> as it is.</param>
    /// <param name="lookups">Content of <see cref="LookupsEntryName"/>.</param>
    /// <param name="addFilesAsync">Optional callback adding further entries (attachments).</param>
    /// <param name="cancellationToken">Cancels building and uploading.</param>
    public static async Task<Result> StoreAsync(
        BlobManager target,
        string baseName,
        Stream data,
        JsonObject lookups,
        Func<ZipArchive, CancellationToken, Task>? addFilesAsync,
        CancellationToken cancellationToken
    )
    {
        var archiveName = GetArchiveName(baseName);
        var zipPath = Path.Combine(Path.GetTempPath(), $"{baseName}_{Guid.NewGuid():N}.zip");

        try
        {
            long jsonSize;
            await using (var zipArchive = await ZipFile.OpenAsync(zipPath, ZipArchiveMode.Create, cancellationToken))
            {
                jsonSize = await WriteDataEntryAsync(zipArchive, data, cancellationToken);
                jsonSize += await WriteJsonEntryAsync(zipArchive, lookups, LookupsEntryName, cancellationToken);

                if (addFilesAsync is not null)
                    await addFilesAsync(zipArchive, cancellationToken);
            }

            var archiveSize = new FileInfo(zipPath).Length;

            await using (var reader = File.OpenRead(zipPath))
                await target.UploadAsync(archiveName, reader, overwrite: true, cancellationToken);

            return new Result(archiveName, jsonSize, archiveSize);
        }
        finally
        {
            if (File.Exists(zipPath))
                File.Delete(zipPath);
        }
    }

    private static async Task<long> WriteDataEntryAsync(ZipArchive archive, Stream data, CancellationToken cancellationToken)
    {
        await using var entryStream = await CreateEntryAsync(archive, DataEntryName, cancellationToken);
        await using var counter = new CountingStream(entryStream, leaveOpen: true);

        await data.CopyToAsync(counter, cancellationToken);
        return counter.BytesWritten;
    }

    private static async Task<long> WriteJsonEntryAsync(ZipArchive archive, JsonNode json, string entryName, CancellationToken cancellationToken)
    {
        await using var entryStream = await CreateEntryAsync(archive, entryName, cancellationToken);
        await using var counter = new CountingStream(entryStream, leaveOpen: true);
        await using (var writer = new Utf8JsonWriter(counter, CompressedJsonWriter.Options))
        {
            json.WriteTo(writer);
            await writer.FlushAsync(cancellationToken);
        }

        return counter.BytesWritten;
    }

    private static Task<Stream> CreateEntryAsync(ZipArchive archive, string entryName, CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(entryName);
        entry.LastWriteTime = DateTimeOffset.UtcNow;

        return entry.OpenAsync(cancellationToken);
    }
}
