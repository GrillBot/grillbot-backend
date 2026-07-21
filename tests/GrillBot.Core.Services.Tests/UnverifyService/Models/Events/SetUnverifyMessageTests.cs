using UnverifyService.Models.Events;
using UnverifyService.Models.Request;

namespace GrillBot.Core.Services.Tests.UnverifyService.Models.Events;

[TestClass]
public class SetUnverifyMessageTests
{
    [TestMethod]
    public void DefaultConstructor_InitializesProperties()
    {
        var message = new SetUnverifyMessage();

        Assert.AreEqual("Unverify", message.Topic);
        Assert.AreEqual("SetUnverify", message.Queue);

        Assert.AreEqual(0UL, message.GuildId);
        Assert.AreEqual(0UL, message.UserId);
        Assert.AreEqual(0UL, message.ChannelId);
        Assert.AreEqual(0UL, message.MessageId);
        Assert.AreEqual(default, message.EndAtUtc);
        Assert.IsNull(message.Reason);
        Assert.IsFalse(message.TestRun);
        Assert.IsFalse(message.IsSelfUnverify);
        Assert.IsNotNull(message.RequiredKeepables);
        Assert.IsEmpty(message.RequiredKeepables);
    }

    [TestMethod]
    public void Constructor_FromUnverifyRequest_CopiesAllProperties()
    {
        var now = DateTime.UtcNow;
        var request = new UnverifyRequest
        {
            GuildId = 1,
            UserId = 2,
            ChannelId = 3,
            MessageId = 4,
            EndAtUtc = now,
            Reason = "reason",
            TestRun = true,
            IsSelfUnverify = true,
            RequiredKeepables = ["A", "B"]
        };

        var message = new SetUnverifyMessage(request);

        Assert.AreEqual(request.GuildId, message.GuildId);
        Assert.AreEqual(request.UserId, message.UserId);
        Assert.AreEqual(request.ChannelId, message.ChannelId);
        Assert.AreEqual(request.MessageId, message.MessageId);
        Assert.AreEqual(request.EndAtUtc, message.EndAtUtc);
        Assert.AreEqual(request.Reason, message.Reason);
        Assert.AreEqual(request.TestRun, message.TestRun);
        Assert.AreEqual(request.IsSelfUnverify, message.IsSelfUnverify);
        CollectionAssert.AreEqual(request.RequiredKeepables, message.RequiredKeepables);
    }

    [TestMethod]
    public void Constructor_FromUnverifyRequest_NullRequiredKeepables_InitializesEmptyList()
    {
        var request = new UnverifyRequest
        {
            RequiredKeepables = null!
        };

        var message = new SetUnverifyMessage(request);

        Assert.IsNotNull(message.RequiredKeepables);
        Assert.IsEmpty(message.RequiredKeepables);
    }
}