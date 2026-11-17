using System.IO.Compression;
using System.Text.Json.Nodes;

namespace GrillBot.Core.IO;

/// <summary>
/// Read side of <see cref="CompressedJsonWriter"/>.
/// </summary>
public static class CompressedJson
{
    public static Stream OpenRead(byte[] compressedContent)
        => new BrotliStream(new MemoryStream(compressedContent, writable: false), CompressionMode.Decompress);

    public static async Task<JsonNode?> ParseAsync(byte[] compressedContent, CancellationToken cancellationToken = default)
    {
        await using var stream = OpenRead(compressedContent);
        return await JsonNode.ParseAsync(stream, cancellationToken: cancellationToken);
    }
}
