using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GrillBot.Core.IO;

namespace GrillBot.Core.Tests.IO;

[TestClass]
public class CompressedJsonTests
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public async Task RoundTrip_ReturnsWrittenJsonAndExactSizes()
    {
        using var target = new MemoryStream();
        long size;
        long compressedSize;

        await using (var writer = new CompressedJsonWriter(target))
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

            await writer.CompleteAsync(TestContext.CancellationToken);
            size = writer.UncompressedSize;
            compressedSize = writer.CompressedSize;
        }

        Assert.IsTrue(target.CanWrite, "The target stream must stay open.");
        Assert.AreEqual(target.Length, compressedSize);

        target.Position = 0;
        await using var stream = CompressedJson.OpenRead(target, leaveOpen: true);
        var json = await JsonNode.ParseAsync(stream, cancellationToken: TestContext.CancellationToken);
        Assert.IsNotNull(json);
        Assert.AreEqual("Archivation", json["Name"]!.GetValue<string>());
        Assert.AreEqual(5000, json["Items"]!.AsArray().Count);
        Assert.AreEqual(4999, json["Items"]![4999]!.GetValue<int>());

        Assert.AreEqual(Encoding.UTF8.GetByteCount(json.ToJsonString(new JsonSerializerOptions { Encoder = CompressedJsonWriter.Options.Encoder })), size);
        Assert.IsLessThan(size, compressedSize);
    }

    [TestMethod]
    public async Task NonAsciiText_IsNotEscaped()
    {
        using var target = new MemoryStream();
        await using (var writer = new CompressedJsonWriter(target))
        {
            writer.Writer.WriteStartObject();
            writer.Writer.WriteString("Name", "Příliš žluťoučký kůň");
            writer.Writer.WriteEndObject();

            await writer.CompleteAsync(TestContext.CancellationToken);
        }

        target.Position = 0;
        await using var stream = CompressedJson.OpenRead(target, leaveOpen: true);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var raw = await reader.ReadToEndAsync(TestContext.CancellationToken);

        Assert.AreEqual("{\"Name\":\"Příliš žluťoučký kůň\"}", raw);
    }

    [TestMethod]
    public async Task CompleteAsync_Twice_Throws()
    {
        using var target = new MemoryStream();
        await using var writer = new CompressedJsonWriter(target);
        writer.Writer.WriteStartObject();
        writer.Writer.WriteEndObject();

        await writer.CompleteAsync(TestContext.CancellationToken);
        await Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => writer.CompleteAsync(TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task Dispose_WithoutComplete_ClosesTheWriter_ButNotTheTarget()
    {
        using var target = new MemoryStream();
        var writer = new CompressedJsonWriter(target);
        writer.Writer.WriteStartObject();

        await writer.DisposeAsync();

        await Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => writer.CompleteAsync(TestContext.CancellationToken));
        Assert.IsTrue(target.CanWrite);
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
