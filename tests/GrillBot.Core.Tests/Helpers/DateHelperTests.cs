using GrillBot.Core.Helpers;

namespace GrillBot.Core.Tests.Helpers;

[TestClass]
public class DateHelperTests
{
    [TestMethod]
    public void EndOfDay()
        => CheckValue(DateHelper.EndOfDay, DateTimeKind.Local);

    [TestMethod]
    public void EndOfDay_Utc()
        => CheckValue(DateHelper.EndOfDayUtc, DateTimeKind.Utc);

    private static void CheckValue(DateTime result, DateTimeKind expectedKind)
    {
        Assert.AreEqual(expectedKind, result.Kind);
        Assert.AreEqual(23, result.Hour);
        Assert.AreEqual(59, result.Minute);
        Assert.AreEqual(59, result.Second);
        Assert.AreEqual(999, result.Millisecond);
    }
}
