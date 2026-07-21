using GrillBot.Core.IO;

namespace GrillBot.Core.Tests.IO;

[TestClass]
public class TemporaryFileTests
{
    public TestContext TestContext { get; set; }

    private static async Task InitFileAsync(TemporaryFile file)
        => await File.WriteAllTextAsync(file.Path, "ASDF");

    [TestMethod]
    public async Task CreateAndDisposeAsync()
    {
        var file = new TemporaryFile("txt");
        await InitFileAsync(file);

        file.Dispose();
        Assert.IsFalse(File.Exists(file.Path));
    }

    [TestMethod]
    public void ChangeExtension()
    {
        const string newExtension = "jpg";

        using var file = new TemporaryFile("txt");
        file.ChangeExtension(newExtension);

        Assert.AreEqual($".{newExtension}", Path.GetExtension(file.Path));
    }

    [TestMethod]
    public async Task ReadAllBytesAsync()
    {
        using var file = new TemporaryFile("txt");
        await InitFileAsync(file);

        var result = await file.ReadAllBytesAsync(TestContext.CancellationToken);
        Assert.HasCount(4, result);
    }

    [TestMethod]
    public async Task ReadAllLinesAsync()
    {
        using var file = new TemporaryFile("txt");
        await InitFileAsync(file);

        var result = await file.ReadAllLinesAsync(TestContext.CancellationToken);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task ReadAllTextAsync()
    {
        using var file = new TemporaryFile("txt");
        await InitFileAsync(file);

        var result = await file.ReadAllTextAsync(TestContext.CancellationToken);
        Assert.AreEqual(4, result.Length);
    }

    [TestMethod]
    public async Task WriteAllBytesAsync()
    {
        using var file = new TemporaryFile("txt");

        await file.WriteAllBytesAsync([1, 2, 3, 4], TestContext.CancellationToken);
        var result = await file.ReadAllBytesAsync(TestContext.CancellationToken);

        Assert.HasCount(4, result);
    }

    [TestMethod]
    public async Task WriteAllLinesAsync()
    {
        using var file = new TemporaryFile("txt");

        await file.WriteAllLinesAsync(["1"], TestContext.CancellationToken);
        var result = await file.ReadAllLinesAsync(TestContext.CancellationToken);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task WriteAllTextAsync()
    {
        using var file = new TemporaryFile("txt");

        await file.WriteAllTextAsync("text", TestContext.CancellationToken);
        var result = await file.ReadAllTextAsync(TestContext.CancellationToken);

        Assert.AreEqual("text", result);
    }

    [TestMethod]
    public void FilenameProperty()
    {
        using var file = new TemporaryFile("txt");

        Assert.IsFalse(string.IsNullOrEmpty(file.Filename));
    }

    [TestMethod]
    public async Task WriteStreamAsync()
    {
        byte[] data = [1, 2, 3, 4];

        await using var sourceStream = new MemoryStream(data);
        using var file = new TemporaryFile("txt");

        await file.WriteStreamAsync(sourceStream, TestContext.CancellationToken);
        var result = await file.ReadAllBytesAsync(TestContext.CancellationToken);

        Assert.HasCount(data.Length, result);
    }
}
