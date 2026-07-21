namespace GrillBot.Core.Extensions;

public static class StreamExtensions
{
    public static byte[] ToByteArray(this Stream stream)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);

        return ms.ToArray();
    }

    public static async Task<byte[]> ToByteArrayAsync(this Stream stream, CancellationToken cancellationToken = default)
    {
        await using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);

        return ms.ToArray();
    }
}
