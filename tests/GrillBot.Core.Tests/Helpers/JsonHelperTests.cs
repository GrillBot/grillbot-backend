using GrillBot.Core.Helpers;
using GrillBot.Core.Tests.Shared.TestCore;

namespace GrillBot.Core.Tests.Helpers;

[TestClass]
public class JsonHelperTests
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public async Task SerializeAsync()
    {
        var data = new FlattenClass() { X = 5 };
        var json = await JsonHelper.SerializeAsync(data, TestContext.CancellationToken);

        Assert.IsNotNull(json);
        Assert.AreNotEqual("{}", json);
        Assert.AreEqual("{\"X\":5,\"SubItems\":null}", json);
    }
}
