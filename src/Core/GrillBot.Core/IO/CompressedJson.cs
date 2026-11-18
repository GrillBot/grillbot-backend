using System.IO.Compression;

namespace GrillBot.Core.IO;

/// <summary>
/// Read side of <see cref="CompressedJsonWriter"/>.
/// </summary>
public static class CompressedJson
{
    /// <summary>A stream of the uncompressed JSON. Disposing it disposes <paramref name="compressed"/> unless <paramref name="leaveOpen"/>.</summary>
    public static Stream OpenRead(Stream compressed, bool leaveOpen = false)
        => new BrotliStream(compressed, CompressionMode.Decompress, leaveOpen);
}
