using GrillBot.Core.Extensions;
using GrillBot.Core.Tests.Shared.TestCore;

namespace GrillBot.Core.Tests.Extensions;

[TestClass]
public class EnumerableExtensionsTests
{
    [TestMethod]
    public async Task FindAllAsync()
    {
        var collection = new List<int> { 1, 2, 3, 4, 5 };

        var result = await collection.FindAllAsync(i => Task.FromResult(i % 2 == 0));
        var expected = new List<int> { 2, 4 };

        Assert.HasCount(2, result);
        Assert.IsTrue(result.SequenceEqual(expected));
    }

    [TestMethod]
    public void Flatten()
    {
        var collection = new List<FlattenClass>
        {
            new()
            {
                X = 1,
                SubItems = [new() { X = 2 }]
            },
            new()
            {
                X = 3,
                SubItems = [new() { X = 4 }]
            },
            new() { X = 5 }
        };

        var expected = new List<FlattenClass>
        {
            new() { X = 1 },
            new() { X = 2 },
            new() { X = 3 },
            new() { X = 4 },
            new() { X = 5 }
        };

        var result = collection.Flatten(o => o.SubItems).ToList();

        Assert.HasCount(expected.Count, result);
        for (var i = 0; i < result.Count; i++)
            Assert.AreEqual(expected[i].X, result[i].X);
    }

    [TestMethod]
    public void IsSequenceEqual()
    {
        var first = new[] { 1, 3, 2, 8, 4 };
        var second = new[] { 1, 2, 3, 4, 8 };

        var result = first.IsSequenceEqual(second);

        Assert.IsTrue(result);
    }
}
