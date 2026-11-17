using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using GrillBot.Common.FileStorage;
using GrillBot.Core.IO;

namespace GrillBot.App.Handlers.AsyncMessaging.Archivation;

/// <summary>
/// Builds an archive ZIP in a temporary file and uploads it. The JSON is written straight into the ZIP entry,
/// never materialized as a string, and other files are streamed in one at a time.
/// </summary>
public static class ArchiveStorage
{
    public sealed record Result(string ArchiveName, long JsonSize, long ArchiveSize);

    /// <summary>
    /// Writes the JSON (and optional further entries) into a temporary ZIP, uploads it to <paramref name="target"/>
    /// under <c>{baseName}.zip</c>, replacing an earlier upload of the same name, and deletes the temporary file.
    /// </summary>
    /// <returns>The uploaded archive name, the uncompressed JSON size and the ZIP size, both in bytes.</returns>
    /// <param name="target">Blob container the archive is uploaded to.</param>
    /// <param name="baseName">Name of the archive and of the JSON inside it, without extension. Must be deterministic per archive, so a retried upload replaces the first one.</param>
    /// <param name="json">The archive content.</param>
    /// <param name="addFilesAsync">Optional callback adding further entries (attachments).</param>
    /// <param name="cancellationToken">Cancels building and uploading.</param>
    public static async Task<Result> StoreAsync(
        BlobManager target,
        string baseName,
        JsonNode json,
        Func<ZipArchive, CancellationToken, Task>? addFilesAsync,
        CancellationToken cancellationToken
    )
    {
        var archiveName = $"{baseName}.zip";
        var zipPath = Path.Combine(Path.GetTempPath(), $"{baseName}_{Guid.NewGuid():N}.zip");

        try
        {
            long jsonSize;
            await using (var zipArchive = await ZipFile.OpenAsync(zipPath, ZipArchiveMode.Create, cancellationToken))
            {
                jsonSize = await WriteJsonEntryAsync(zipArchive, json, $"{baseName}.json", cancellationToken);

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

    private static async Task<long> WriteJsonEntryAsync(ZipArchive archive, JsonNode json, string entryName, CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(entryName);
        entry.LastWriteTime = DateTimeOffset.UtcNow;

        await using var entryStream = await entry.OpenAsync(cancellationToken);
        await using var counter = new CountingStream(entryStream, leaveOpen: true);
        await using (var writer = new Utf8JsonWriter(counter, CompressedJsonWriter.Options))
        {
            json.WriteTo(writer);
            await writer.FlushAsync(cancellationToken);
        }

        return counter.BytesWritten;
    }
}
