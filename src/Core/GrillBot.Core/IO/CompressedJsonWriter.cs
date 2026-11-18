using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace GrillBot.Core.IO;

/// <summary>
/// Streams JSON through Brotli into a target stream (typically a shared file), so the document is never held
/// in memory. Write with <see cref="Writer"/>, call <see cref="FlushIfNeededAsync"/> between items to keep the
/// writer's own buffer small, and finish with <see cref="CompleteAsync"/>. The target stream is left open.
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

    private readonly CountingStream _compressedCounter;
    private readonly BrotliStream _compressor;
    private readonly CountingStream _uncompressedCounter;
    private bool _completed;

    public Utf8JsonWriter Writer { get; }

    /// <summary>Uncompressed bytes written so far (exact after <see cref="CompleteAsync"/>).</summary>
    public long UncompressedSize => _uncompressedCounter.BytesWritten;

    /// <summary>Compressed bytes written to the target so far (exact after <see cref="CompleteAsync"/>).</summary>
    public long CompressedSize => _compressedCounter.BytesWritten;

    public CompressedJsonWriter(Stream target, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        _compressedCounter = new CountingStream(target, leaveOpen: true);
        _compressor = new BrotliStream(_compressedCounter, compressionLevel, leaveOpen: true);
        _uncompressedCounter = new CountingStream(_compressor, leaveOpen: true);
        Writer = new Utf8JsonWriter(_uncompressedCounter, Options);
    }

    public async Task FlushIfNeededAsync(CancellationToken cancellationToken = default)
    {
        if (Writer.BytesPending >= FlushThreshold)
            await Writer.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Flushes everything and finishes the compressed stream. The writer can not be used afterwards;
    /// the target stream stays open.
    /// </summary>
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_completed, this);

        await Writer.FlushAsync(cancellationToken);
        await CloseStreamsAsync();
    }

    private async Task CloseStreamsAsync()
    {
        await Writer.DisposeAsync();
        await _uncompressedCounter.DisposeAsync();
        await _compressor.DisposeAsync();
        await _compressedCounter.DisposeAsync();
        _completed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_completed)
            await CloseStreamsAsync();
    }
}
