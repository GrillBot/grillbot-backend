namespace GrillBot.Core.Extensions;

public static class DateTimeExtensions
{
    public static DateTime WithKind(this DateTime date, DateTimeKind kind)
        => new(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, date.Microsecond, kind);

    public static DateOnly ToDateOnly(this DateTime dateTime)
        => new(dateTime.Year, dateTime.Month, dateTime.Day);

    public static DateOnly ToDateOnly(this DateTimeOffset dateTime)
        => new(dateTime.Year, dateTime.Month, dateTime.Day);
}
