#pragma warning disable IDE0290 // Use primary constructor
namespace GrillBot.Core.IO;

public sealed class TemporaryFile : IDisposable
{
    public string Path { get; private set; }

    public string Filename => System.IO.Path.GetFileName(Path);

    public TemporaryFile(string extension)
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{System.IO.Path.GetRandomFileName()}.{extension}");
    }

    public void ChangeExtension(string newExtension)
        => Path = System.IO.Path.ChangeExtension(Path, newExtension);

    public Task<byte[]> ReadAllBytesAsync(CancellationToken cancellationToken = default) => File.ReadAllBytesAsync(Path, cancellationToken);
    public Task<string[]> ReadAllLinesAsync(CancellationToken cancellationToken = default) => File.ReadAllLinesAsync(Path, cancellationToken);
    public Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default) => File.ReadAllTextAsync(Path, cancellationToken);
    public Task WriteAllBytesAsync(byte[] bytes, CancellationToken cancellationToken = default) => File.WriteAllBytesAsync(Path, bytes, cancellationToken);
    public Task WriteAllLinesAsync(IEnumerable<string> lines, CancellationToken cancellationToken = default) => File.WriteAllLinesAsync(Path, lines, cancellationToken);
    public Task WriteAllTextAsync(string content, CancellationToken cancellationToken = default) => File.WriteAllTextAsync(Path, content, cancellationToken);

    public async Task WriteStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        await using var fileStream = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite);

        await stream.CopyToAsync(fileStream, cancellationToken);
        await fileStream.FlushAsync(cancellationToken);
    }

    public void Dispose()
    {
        if (File.Exists(Path))
            File.Delete(Path);
    }

    public override string ToString() => Path;
}
