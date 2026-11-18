using GrillBot.Core.IO.SharedStorage;
using Microsoft.Extensions.Options;

namespace GrillBot.Core.Tests.IO;

[TestClass]
public class SharedFileStorageTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 18, 2, 0, 0, TimeSpan.Zero);

    private string _root = null!;
    private SharedFileStorage _storage = null!;

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void Initialize()
    {
        _root = Path.Combine(Path.GetTempPath(), $"grillbot-shared-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_root);
        _storage = new SharedFileStorage(_root, new FixedTimeProvider(Now));
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    [TestMethod]
    public async Task Commit_MakesTheFileVisibleOnlyAfterwards()
    {
        string name;
        await using (var writer = _storage.Create("AuditLog", ".json.br"))
        {
            name = writer.Name;
            await writer.Stream.WriteAsync(new byte[] { 1, 2, 3 }, TestContext.CancellationToken);

            Assert.IsFalse(_storage.Exists(name));
            Assert.IsTrue(File.Exists(Path.Combine(_root, name + SharedFileStorage.PartialExtension)));

            Assert.AreEqual(3, await writer.CommitAsync(TestContext.CancellationToken));
        }

        Assert.IsTrue(_storage.Exists(name));
        Assert.IsFalse(File.Exists(Path.Combine(_root, name + SharedFileStorage.PartialExtension)));
        Assert.StartsWith("AuditLog_20261118_020000_", name);
        Assert.EndsWith(".json.br", name);

        await using var reader = _storage.OpenRead(name);
        var buffer = new byte[3];
        await reader.ReadExactlyAsync(buffer, TestContext.CancellationToken);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, buffer);
    }

    [TestMethod]
    public async Task Dispose_WithoutCommit_DeletesThePartialFile()
    {
        string name;
        await using (var writer = _storage.Create("AuditLog", ".json.br"))
        {
            name = writer.Name;
            await writer.Stream.WriteAsync(new byte[] { 1 }, TestContext.CancellationToken);
        }

        Assert.IsFalse(_storage.Exists(name));
        Assert.IsEmpty(Directory.GetFiles(_root));
    }

    [TestMethod]
    public async Task Delete_RemovesTheFile_AndIgnoresMissingOnes()
    {
        await using var writer = _storage.Create("Unverify", ".json.br");
        await writer.CommitAsync(TestContext.CancellationToken);

        _storage.Delete(writer.Name);
        _storage.Delete(writer.Name);

        Assert.IsFalse(_storage.Exists(writer.Name));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(".")]
    [DataRow("..")]
    [DataRow("../secret.json")]
    [DataRow("..\\secret.json")]
    [DataRow("sub/file.json")]
    [DataRow("/etc/passwd")]
    [DataRow("C:\\Windows\\win.ini")]
    [DataRow("AuditLog_x.json.br.partial")]
    public void NamesOutsideOfTheDirectory_AreRejected(string name)
    {
        Assert.ThrowsExactly<ArgumentException>(() => _storage.Exists(name));
        Assert.ThrowsExactly<ArgumentException>(() => _storage.OpenRead(name));
        Assert.ThrowsExactly<ArgumentException>(() => _storage.Delete(name));
    }

    [TestMethod]
    [DataRow("Audit Log")]
    [DataRow("../AuditLog")]
    [DataRow("")]
    public void Create_RejectsInvalidPrefix(string prefix)
        => Assert.ThrowsExactly<ArgumentException>(() => _storage.Create(prefix, ".json.br"));

    [TestMethod]
    public async Task DeleteExpired_RemovesOnlyOldFilesOfThePrefix()
    {
        var oldFile = await CreateCommittedAsync("AuditLog");
        var newFile = await CreateCommittedAsync("AuditLog");
        var otherPrefix = await CreateCommittedAsync("Unverify");
        var oldPartial = Path.Combine(_root, "AuditLog_20261101_000000_x.json.br.partial");
        await File.WriteAllBytesAsync(oldPartial, [], TestContext.CancellationToken);

        var old = Now.UtcDateTime.AddDays(-4);
        File.SetLastWriteTimeUtc(Path.Combine(_root, oldFile), old);
        File.SetLastWriteTimeUtc(Path.Combine(_root, otherPrefix), old);
        File.SetLastWriteTimeUtc(oldPartial, old);
        File.SetLastWriteTimeUtc(Path.Combine(_root, newFile), Now.UtcDateTime.AddHours(-1));

        Assert.AreEqual(2, _storage.DeleteExpired("AuditLog", TimeSpan.FromDays(3)));

        Assert.IsFalse(_storage.Exists(oldFile));
        Assert.IsFalse(File.Exists(oldPartial));
        Assert.IsTrue(_storage.Exists(newFile));
        Assert.IsTrue(_storage.Exists(otherPrefix));
    }

    [TestMethod]
    public void DeleteExpired_MissingDirectory_ReturnsZero()
    {
        Directory.Delete(_root);
        Assert.AreEqual(0, _storage.DeleteExpired("AuditLog", TimeSpan.FromDays(3)));
    }

    [TestMethod]
    public void Validator_AcceptsWritableDirectory_AndLeavesNoProbe()
    {
        var validator = new SharedStorageOptionsValidator();

        Assert.IsTrue(validator.Validate(null, new SharedStorageOptions { Path = _root }).Succeeded);
        Assert.IsEmpty(Directory.GetFiles(_root));
    }

    [TestMethod]
    public void Validator_RejectsMissingDirectory_WithoutCreatingIt()
    {
        var missing = Path.Combine(_root, "not-mounted");

        var result = new SharedStorageOptionsValidator().Validate(null, new SharedStorageOptions { Path = missing });

        Assert.IsTrue(result.Failed);
        Assert.IsFalse(Directory.Exists(missing));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("relative/path")]
    public void Validator_RejectsEmptyOrRelativePath(string path)
        => Assert.IsTrue(new SharedStorageOptionsValidator().Validate(null, new SharedStorageOptions { Path = path }).Failed);

    [TestMethod]
    public void RootPath_IgnoresTrailingSeparator()
    {
        var storage = new SharedFileStorage(Options.Create(new SharedStorageOptions { Path = _root + Path.DirectorySeparatorChar }));
        Assert.AreEqual(Path.GetFullPath(_root), storage.RootPath);
        Assert.IsFalse(storage.Exists("AuditLog_x.json.br"));
    }

    private async Task<string> CreateCommittedAsync(string prefix)
    {
        await using var writer = _storage.Create(prefix, ".json.br");
        await writer.CommitAsync(TestContext.CancellationToken);
        return writer.Name;
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
