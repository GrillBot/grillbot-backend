using GrillBot.Core.Extensions;

namespace GrillBot.Core.Tests.Extensions;

[TestClass]
public class StringExtensionsTests
{
    [TestMethod]
    public void ToUlong_Null()
    {
        var result = ((string?)null).ToUlong();
        Assert.AreEqual(default, result);
    }

    [TestMethod]
    public void ToUlong()
    {
        var result = "123456".ToUlong();
        Assert.AreEqual(123456UL, result);
    }

    [TestMethod]
    public void ToInt_Null()
    {
        var result = ((string?)null).ToInt();
        Assert.AreEqual(default, result);
    }

    [TestMethod]
    public void ToInt()
    {
        var result = "123456".ToInt();
        Assert.AreEqual(123456, result);
    }

    [TestMethod]
    public void Cut_Null()
    {
        var result = ((string?)null).Cut(5);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Cut_WithoutDots()
    {
        var result = "ABCDEFGH".Cut(4, true);
        Assert.AreEqual("ABCD", result);
    }

    [TestMethod]
    public void Cut_WithDots()
    {
        var result = "ABCDEFGH".Cut(4);
        Assert.AreEqual("A...", result);
    }
}
