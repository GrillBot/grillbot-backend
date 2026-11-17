using System.Text;
using GrillBot.Core.IO;

namespace GrillBot.Core.Tests.IO;

[TestClass]
public class CompressedJsonTests
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public async Task RoundTrip_ReturnsWrittenJsonAndExactUncompressedSize()
    {
        byte[] content;
        long size;

        await using (var writer = new CompressedJsonWriter())
        {
            writer.Writer.WriteStartObject();
            writer.Writer.WriteString("Name", "Archivation");
            writer.Writer.WriteStartArray("Items");
            for (var i = 0; i < 5000; i++)
            {
                writer.Writer.WriteNumberValue(i);
                await writer.FlushIfNeededAsync(TestContext.CancellationToken);
            }

            writer.Writer.WriteEndArray();
            writer.Writer.WriteEndObject();

            content = await writer.CompleteAsync(TestContext.CancellationToken);
            size = writer.UncompressedSize;
        }

        var json = await CompressedJson.ParseAsync(content, TestContext.CancellationToken);
        Assert.IsNotNull(json);
        Assert.AreEqual("Archivation", json["Name"]!.GetValue<string>());
        Assert.AreEqual(5000, json["Items"]!.AsArray().Count);
        Assert.AreEqual(4999, json["Items"]![4999]!.GetValue<int>());

        await using var stream = CompressedJson.OpenRead(content);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var raw = await reader.ReadToEndAsync(TestContext.CancellationToken);

        Assert.AreEqual(Encoding.UTF8.GetByteCount(raw), size);
        Assert.IsLessThan(size, content.LongLength);
    }

    [TestMethod]
    public async Task NonAsciiText_IsNotEscaped()
    {
        await using var writer = new CompressedJsonWriter();
        writer.Writer.WriteStartObject();
        writer.Writer.WriteString("Name", "Příliš žluťoučký kůň");
        writer.Writer.WriteEndObject();

        var content = await writer.CompleteAsync(TestContext.CancellationToken);

        await using var stream = CompressedJson.OpenRead(content);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var raw = await reader.ReadToEndAsync(TestContext.CancellationToken);

        Assert.AreEqual("{\"Name\":\"Příliš žluťoučký kůň\"}", raw);
    }

    [TestMethod]
    public async Task CompleteAsync_Twice_Throws()
    {
        await using var writer = new CompressedJsonWriter();
        writer.Writer.WriteStartObject();
        writer.Writer.WriteEndObject();

        await writer.CompleteAsync(TestContext.CancellationToken);
        await Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => writer.CompleteAsync(TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task Dispose_WithoutComplete_ClosesTheWriter()
    {
        var writer = new CompressedJsonWriter();
        writer.Writer.WriteStartObject();

        await writer.DisposeAsync();

        await Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => writer.CompleteAsync(TestContext.CancellationToken));
    }

    [TestMethod]
    public void CountingStream_CountsAllWriteOverloads()
    {
        using var inner = new MemoryStream();
        using var stream = new CountingStream(inner, leaveOpen: true);

        stream.Write([1, 2, 3], 0, 3);
        stream.Write([4, 5]);
        stream.WriteByte(6);

        Assert.AreEqual(6, stream.BytesWritten);
        Assert.AreEqual(6, inner.Length);
        Assert.IsFalse(stream.CanRead);
    }

    [TestMethod]
    public async Task CountingStream_CountsAsyncWrites_AndClosesInnerUnlessLeftOpen()
    {
        var inner = new MemoryStream();
        var stream = new CountingStream(inner);

        await stream.WriteAsync(new byte[] { 1, 2, 3, 4 }, TestContext.CancellationToken);
        Assert.AreEqual(4, stream.BytesWritten);

        await stream.DisposeAsync();
        Assert.IsFalse(inner.CanWrite);
    }
}
