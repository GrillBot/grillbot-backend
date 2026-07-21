using GrillBot.Core.Extensions;

namespace GrillBot.Core.Tests.Extensions;

[TestClass]
public class BitMaskExtensionsTests
{
    [TestMethod]
    public void Int_Set()
    {
        var result = 1.UpdateFlags(2, true);
        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public void Int_Reset()
    {
        var result = 3.UpdateFlags(2, false);
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void Long_Set()
    {
        var result = 1L.UpdateFlags(2L, true);
        Assert.AreEqual(3L, result);
    }

    [TestMethod]
    public void Long_Reset()
    {
        var result = 3L.UpdateFlags(2L, false);
        Assert.AreEqual(1L, result);
    }
}
