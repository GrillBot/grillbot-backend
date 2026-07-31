using Discord;
using NSubstitute;
using GrillBot.Contracts.UserManagement.Events;

namespace GrillBot.Contracts.Tests.UserManagement.Events;

[TestClass]
public class NicknameChangedMessageTests
{
    [TestMethod]
    public void Constructor_SetsAllProperties()
    {
        var message = new NicknameChangedMessage(123UL, 456UL, "oldNick", "newNick");

        Assert.AreEqual(123UL, message.GuildId);
        Assert.AreEqual(456UL, message.UserId);
        Assert.AreEqual("oldNick", message.NicknameBefore);
        Assert.AreEqual("newNick", message.NicknameAfter);
    }

    [TestMethod]
    public void Create_CreatesMessageFromUsers()
    {
        var userBefore = Substitute.For<IGuildUser>();
        var userAfter = Substitute.For<IGuildUser>();

        userBefore.Guild.Id.Returns(789UL);
        userBefore.Id.Returns(1011UL);
        userBefore.Nickname.Returns("beforeNick");
        userAfter.Nickname.Returns("afterNick");

        var message = NicknameChangedMessage.Create(userBefore, userAfter);

        Assert.AreEqual(789UL, message.GuildId);
        Assert.AreEqual(1011UL, message.UserId);
        Assert.AreEqual("beforeNick", message.NicknameBefore);
        Assert.AreEqual("afterNick", message.NicknameAfter);
    }

    [TestMethod]
    public void Equality_ComparesByValue()
    {
        Assert.AreEqual(
            new NicknameChangedMessage(1UL, 2UL, "a", "b"),
            new NicknameChangedMessage(1UL, 2UL, "a", "b")
        );
    }
}
