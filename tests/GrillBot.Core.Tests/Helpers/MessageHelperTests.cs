using GrillBot.Core.Helpers;

namespace GrillBot.Core.Tests.Helpers;

[TestClass]
public class MessageHelperTests
{
    [TestMethod]
    public void DiscordMessageUriRegex()
    {
        var result = MessageHelper.DiscordMessageUriRegex();

        Assert.IsNotNull(result);
        Assert.Contains("discord\\.com", result.ToString());
        Assert.Contains("channels", result.ToString());
    }
}
