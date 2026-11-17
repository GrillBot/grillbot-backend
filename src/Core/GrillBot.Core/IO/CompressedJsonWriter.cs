using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace GrillBot.Core.IO;

/// <summary>
/// Streams JSON through Brotli into memory, so only the compressed form is ever held in full.
/// Write with <see cref="Writer"/>, call <see cref="FlushIfNeededAsync"/> between items to keep the
/// writer's own buffer small, and finish with <see cref="CompleteAsync"/>.
/// </summary>
public sealed class CompressedJsonWriter : IAsyncDisposable
{
    private const int FlushThreshold = 64 * 1024;

    /// <summary>
    /// Compact output that keeps non-ASCII text (Czech) readable instead of escaping it.
    /// The output is a data file, never embedded in HTML, so relaxed escaping is safe.
    /// </summary>
    public static readonly JsonWriterOptions Options = new()
    {
        Indented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly MemoryStream _buffer = new();
    private readonly BrotliStream _compressor;
    private readonly CountingStream _counter;
    private bool _completed;

    public Utf8JsonWriter Writer { get; }

    /// <summary>Uncompressed bytes written so far (exact after <see cref="CompleteAsync"/>).</summary>
    public long UncompressedSize => _counter.BytesWritten;

    public CompressedJsonWriter(CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        _compressor = new BrotliStream(_buffer, compressionLevel, leaveOpen: true);
        _counter = new CountingStream(_compressor, leaveOpen: true);
        Writer = new Utf8JsonWriter(_counter, Options);
    }

    public async Task FlushIfNeededAsync(CancellationToken cancellationToken = default)
    {
        if (Writer.BytesPending >= FlushThreshold)
            await Writer.FlushAsync(cancellationToken);
    }

    /// <summary>Flushes everything and returns the compressed bytes. The writer can not be used afterwards.</summary>
    public async Task<byte[]> CompleteAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_completed, this);

        await Writer.FlushAsync(cancellationToken);
        await CloseStreamsAsync();

        return _buffer.ToArray();
    }

    private async Task CloseStreamsAsync()
    {
        await Writer.DisposeAsync();
        await _counter.DisposeAsync();
        await _compressor.DisposeAsync();
        _completed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_completed)
            await CloseStreamsAsync();

        await _buffer.DisposeAsync();
    }
}
