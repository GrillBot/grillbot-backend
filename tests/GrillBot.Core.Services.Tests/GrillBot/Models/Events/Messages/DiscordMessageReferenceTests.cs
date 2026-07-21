using Discord;
using GrillBot.Models.Events.Messages;

namespace GrillBot.Core.Services.Tests.GrillBot.Models.Events.Messages;

[TestClass]
public class DiscordMessageReferenceTests
{
    private const ulong TestMessageId = 123456789012345678;
    private const ulong TestChannelId = 234567890123456789;
    private const ulong TestGuildId = 345678901234567890;
    private const bool TestFailIfNotExists = false;
    private const MessageReferenceType TestReferenceType = MessageReferenceType.Default;

    [TestMethod]
    public void DefaultConstructor_SetsDefaultValues()
    {
        var reference = new DiscordMessageReference();

        Assert.IsNull(reference.MessageId);
        Assert.IsNull(reference.ChannelId);
        Assert.IsNull(reference.GuildId);
        Assert.IsTrue(reference.FailIfNotExists);
        Assert.AreEqual(MessageReferenceType.Default, reference.ReferenceType);
    }

    [TestMethod]
    public void ParameterizedConstructor_SetsProperties()
    {
        var reference = new DiscordMessageReference(
            TestMessageId,
            TestChannelId,
            TestGuildId,
            TestFailIfNotExists,
            TestReferenceType
        );

        Assert.AreEqual(TestMessageId, reference.MessageId);
        Assert.AreEqual(TestChannelId, reference.ChannelId);
        Assert.AreEqual(TestGuildId, reference.GuildId);
        Assert.AreEqual(TestFailIfNotExists, reference.FailIfNotExists);
        Assert.AreEqual(TestReferenceType, reference.ReferenceType);
    }

    [TestMethod]
    public void ToDiscordReference_MapsPropertiesCorrectly()
    {
        var reference = new DiscordMessageReference(
            TestMessageId,
            TestChannelId,
            TestGuildId,
            TestFailIfNotExists,
            TestReferenceType
        );

        var discordRef = reference.ToDiscordReference();

        Assert.AreEqual(TestMessageId, discordRef.MessageId.Value);
        Assert.AreEqual(TestChannelId, discordRef.ChannelId);
        Assert.AreEqual(TestGuildId, discordRef.GuildId.Value);
        Assert.AreEqual(TestFailIfNotExists, discordRef.FailIfNotExists.Value);
        Assert.AreEqual(TestReferenceType, discordRef.ReferenceType.Value);
    }
}
