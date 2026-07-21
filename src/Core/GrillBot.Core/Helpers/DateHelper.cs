namespace GrillBot.Core.Helpers;

public static class DateHelper
{
    // ReSharper disable once InconsistentNaming
    private static readonly TimeSpan _endOfDay = new(0, 23, 59, 59, 999);

    public static DateTime EndOfDay => DateTime.Now.Date.Add(_endOfDay);
    public static DateTime EndOfDayUtc => DateTime.UtcNow.Date.Add(_endOfDay);
}
