using System.Text.RegularExpressions;

namespace GrillBot.Core.Helpers;

public static partial class MessageHelper
{
    [GeneratedRegex("https:\\/\\/discord\\.com\\/channels\\/(@me|\\d*)\\/(\\d+)\\/(\\d+)")]
    public static partial Regex DiscordMessageUriRegex();
}
