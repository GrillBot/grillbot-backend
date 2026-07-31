using GrillBot.Contracts.Unverify.Events;
using GrillBot.Contracts.Unverify.Requests;

namespace GrillBot.Contracts.Tests.Unverify.Events;

[TestClass]
public class SetUnverifyMessageTests
{
    [TestMethod]
    public void Constructor_CarriesTheRequestUnchanged()
    {
        var request = new UnverifyRequest
        {
            GuildId = 1,
            UserId = 2,
            ChannelId = 3,
            MessageId = 4,
            EndAtUtc = DateTime.UtcNow,
            Reason = "reason",
            TestRun = true,
            IsSelfUnverify = true,
            RequiredKeepables = ["A", "B"]
        };

        var message = new SetUnverifyMessage(request);

        Assert.AreSame(request, message.Request);
    }

    [TestMethod]
    public void Equality_IsByRequestReference()
    {
        var request = new UnverifyRequest { GuildId = 1, UserId = 2 };

        Assert.AreEqual(new SetUnverifyMessage(request), new SetUnverifyMessage(request));
    }
}
