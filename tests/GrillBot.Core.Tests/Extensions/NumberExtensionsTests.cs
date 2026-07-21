using GrillBot.Core.Extensions;

namespace GrillBot.Core.Tests.Extensions;

[TestClass]
public class NumberExtensionsTests
{
    [TestMethod]
    public void FormatNumber_Int()
    {
        const string expected = "1 234 567";
        var result = 1234567.FormatNumber();

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void FormatNumber_Long()
    {
        const string expected = "9 223 372 036 854 775 807";
        var result = long.MaxValue.FormatNumber();

        Assert.AreEqual(expected, result);
    }
}
