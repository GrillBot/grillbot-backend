namespace GrillBot.Core.Extensions;

public static class StringExtensions
{
    public static ulong ToUlong(this string? str) => string.IsNullOrEmpty(str) ? default : Convert.ToUInt64(str);
    public static int ToInt(this string? str) => string.IsNullOrEmpty(str) ? default : Convert.ToInt32(str);

    public static string? Cut(this string? str, int maxLength, bool withoutDots = false)
    {
        if (str is null) return null;

        var withoutDotsLen = withoutDots ? 0 : 3;
        if (str.Length >= maxLength - withoutDotsLen)
            str = str[..(maxLength - withoutDotsLen)] + (withoutDots ? "" : "...");

        return str;
    }
}
