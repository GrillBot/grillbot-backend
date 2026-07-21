using Discord;
using NSubstitute;
using UserManagementService.Models.Events;

namespace GrillBot.Core.Services.Tests.UserManagementService.Models.Events;

[TestClass]
public class NicknameChangedMessageTests
{
    [TestMethod]
    public void DefaultConstructor_InitializesProperties()
    {
        var message = new NicknameChangedMessage();

        Assert.AreEqual("UserManagement", message.Topic);
        Assert.AreEqual("NicknameChanged", message.Queue);
        Assert.AreEqual(0UL, message.GuildId);
        Assert.AreEqual(0UL, message.UserId);
        Assert.IsNull(message.NicknameBefore);
        Assert.IsNull(message.NicknameAfter);
    }

    [TestMethod]
    public void ParameterizedConstructor_SetsAllProperties()
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
}