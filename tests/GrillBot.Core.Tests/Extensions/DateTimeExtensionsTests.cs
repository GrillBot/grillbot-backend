using GrillBot.Core.Extensions;

namespace GrillBot.Core.Tests.Extensions;

[TestClass]
public class DateTimeExtensionsTests
{
    [TestMethod]
    public void WithKind_LocalToUtc()
    {
        var localTime = new DateTime(2023, 10, 9, 10, 23, 42, DateTimeKind.Local);
        var result = localTime.WithKind(DateTimeKind.Utc);

        Assert.AreEqual(localTime.Year, result.Year);
        Assert.AreEqual(localTime.Month, result.Month);
        Assert.AreEqual(localTime.Day, result.Day);
        Assert.AreEqual(localTime.Hour, result.Hour);
        Assert.AreEqual(localTime.Minute, result.Minute);
        Assert.AreEqual(localTime.Second, result.Second);
        Assert.AreEqual(DateTimeKind.Utc, result.Kind);
    }

    [TestMethod]
    public void WithKind_UtcToLocal()
    {
        var localTime = new DateTime(2023, 10, 9, 10, 23, 42, DateTimeKind.Utc);
        var result = localTime.WithKind(DateTimeKind.Local);

        Assert.AreEqual(localTime.Year, result.Year);
        Assert.AreEqual(localTime.Month, result.Month);
        Assert.AreEqual(localTime.Day, result.Day);
        Assert.AreEqual(localTime.Hour, result.Hour);
        Assert.AreEqual(localTime.Minute, result.Minute);
        Assert.AreEqual(localTime.Second, result.Second);
        Assert.AreEqual(DateTimeKind.Local, result.Kind);
    }

    [TestMethod]
    public void ToDateOnly_WithDateTime()
    {
        var dateTime = new DateTime(2023, 2, 15, 11, 0, 0, DateTimeKind.Local);
        var date = dateTime.ToDateOnly();

        Assert.AreEqual(dateTime.Year, date.Year);
        Assert.AreEqual(dateTime.Month, date.Month);
        Assert.AreEqual(dateTime.Day, date.Day);
    }

    [TestMethod]
    public void ToDateOnly_WithDateTimeOffset()
    {
        var dateTime = new DateTimeOffset(2023, 2, 15, 11, 0, 0, TimeSpan.Zero);
        var date = dateTime.ToDateOnly();

        Assert.AreEqual(dateTime.Year, date.Year);
        Assert.AreEqual(dateTime.Month, date.Month);
        Assert.AreEqual(dateTime.Day, date.Day);
    }
}
