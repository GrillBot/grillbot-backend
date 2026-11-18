namespace GrillBot.Core.IO.SharedStorage;

/// <summary>
/// A shared file being written. The content goes to a <c>.partial</c> file; <see cref="CommitAsync"/> renames
/// it to <see cref="Name"/>, which is atomic on one filesystem. Disposing without committing deletes it.
/// </summary>
public sealed class SharedFileWriter : IAsyncDisposable
{
    private readonly string _finalPath;
    private readonly string _partialPath;
    private readonly FileStream _stream;
    private bool _committed;

    /// <summary>The name to publish. The file exists under it only after <see cref="CommitAsync"/>.</summary>
    public string Name { get; }

    /// <summary>Where to write the content. Do not dispose it; <see cref="CommitAsync"/> closes it.</summary>
    public Stream Stream => _stream;

    internal SharedFileWriter(string name, string finalPath)
    {
        Name = name;
        _finalPath = finalPath;
        _partialPath = finalPath + SharedFileStorage.PartialExtension;
        _stream = new FileStream(_partialPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
    }

    /// <summary>Flushes and closes the content and makes the file visible under <see cref="Name"/>.</summary>
    /// <returns>Size of the file in bytes.</returns>
    public async Task<long> CommitAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_committed, this);

        await _stream.FlushAsync(cancellationToken);
        var size = _stream.Length;
        await _stream.DisposeAsync();

        File.Move(_partialPath, _finalPath, overwrite: false);
        _committed = true;

        return size;
    }

    public async ValueTask DisposeAsync()
    {
        if (_committed)
            return;

        await _stream.DisposeAsync();
        File.Delete(_partialPath);
    }
}
