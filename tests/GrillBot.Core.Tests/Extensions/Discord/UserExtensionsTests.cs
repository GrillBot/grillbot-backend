using Discord;
using GrillBot.Core.Extensions.Discord;
using NSubstitute;

namespace GrillBot.Core.Tests.Extensions.Discord;

[TestClass]
public class UserExtensionsTests
{
    [TestMethod]
    public void IsUser_ReturnsTrue_ForNormalUser()
    {
        var user = Substitute.For<IUser>();
        user.IsBot.Returns(false);
        user.IsWebhook.Returns(false);

        Assert.IsTrue(user.IsUser());
    }

    [TestMethod]
    public void IsUser_ReturnsFalse_ForBot()
    {
        var user = Substitute.For<IUser>();
        user.IsBot.Returns(true);
        user.IsWebhook.Returns(false);

        Assert.IsFalse(user.IsUser());
    }

    [TestMethod]
    public void IsUser_ReturnsFalse_ForWebhook()
    {
        var user = Substitute.For<IUser>();
        user.IsBot.Returns(false);
        user.IsWebhook.Returns(true);

        Assert.IsFalse(user.IsUser());
    }

    [TestMethod]
    public void GetFullName_GuildUserWithNicknameAndGlobalName_ReturnsExpectedFormat()
    {
        var user = Substitute.For<IGuildUser, IUser>();
        user.Nickname.Returns("Nick");
        user.GlobalName.Returns("Global");
        user.Username.Returns("User");
        user.IsBot.Returns(false);
        user.IsWebhook.Returns(false);

        var result = user.GetFullName();

        Assert.AreEqual("Nick (Global / User)", result);
    }

    [TestMethod]
    public void GetFullName_GuildUserWithNicknameAndNoGlobalName_ReturnsNicknameAndUsername()
    {
        var user = Substitute.For<IGuildUser, IUser>();
        user.Nickname.Returns("Nick");
        user.GlobalName.Returns((string)null!);
        user.Username.Returns("User");

        var result = user.GetFullName();

        Assert.AreEqual("Nick (User)", result);
    }

    [TestMethod]
    public void GetFullName_GuildUserWithNicknameAndGlobalNameEqualsUsername_ReturnsNicknameAndUsername()
    {
        var user = Substitute.For<IGuildUser, IUser>();
        user.Nickname.Returns("Nick");
        user.GlobalName.Returns("User");
        user.Username.Returns("User");

        var result = user.GetFullName();

        Assert.AreEqual("Nick (User)", result);
    }

    [TestMethod]
    public void GetFullName_UserWithGlobalNameDifferentThanUsername_ReturnsGlobalNameAndUsername()
    {
        var user = Substitute.For<IUser>();
        user.GlobalName.Returns("Global");
        user.Username.Returns("User");

        var result = user.GetFullName();

        Assert.AreEqual("Global / User", result);
    }

    [TestMethod]
    public void GetFullName_UserWithGlobalNameEqualsUsername_ReturnsUsername()
    {
        var user = Substitute.For<IUser>();
        user.GlobalName.Returns("User");
        user.Username.Returns("User");

        var result = user.GetFullName();

        Assert.AreEqual("User", result);
    }

    [TestMethod]
    public void GetFullName_UserWithNoGlobalName_ReturnsUsername()
    {
        var user = Substitute.For<IUser>();
        user.GlobalName.Returns((string)null!);
        user.Username.Returns("User");

        var result = user.GetFullName();

        Assert.AreEqual("User", result);
    }

    [TestMethod]
    public void GetDisplayName_GuildUserWithNickname_ReturnsNickname()
    {
        var user = Substitute.For<IGuildUser, IUser>();
        user.Nickname.Returns("Nick");
        user.GlobalName.Returns("Global");
        user.Username.Returns("User");

        var result = user.GetDisplayName();

        Assert.AreEqual("Nick", result);
    }

    [TestMethod]
    public void GetDisplayName_GuildUserWithNullNickname_ReturnsGlobalNameIfNotNull()
    {
        var user = Substitute.For<IGuildUser, IUser>();
        user.Nickname.Returns((string?)null);
        user.GlobalName.Returns("Global");
        user.Username.Returns("User");

        var result = user.GetDisplayName();

        Assert.AreEqual("Global", result);
    }

    [TestMethod]
    public void GetDisplayName_GuildUserWithEmptyNickname_ReturnsGlobalNameIfNotNull()
    {
        var user = Substitute.For<IGuildUser, IUser>();
        user.Nickname.Returns(string.Empty);
        user.GlobalName.Returns("Global");
        user.Username.Returns("User");

        var result = user.GetDisplayName();

        Assert.AreEqual("Global", result);
    }

    [TestMethod]
    public void GetDisplayName_UserWithNullGlobalName_ReturnsUsername()
    {
        var user = Substitute.For<IUser>();
        user.GlobalName.Returns((string?)null);
        user.Username.Returns("User");

        var result = user.GetDisplayName();

        Assert.AreEqual("User", result);
    }

    [TestMethod]
    public void GetDisplayName_UserWithEmptyGlobalName_ReturnsUsername()
    {
        var user = Substitute.For<IUser>();
        user.GlobalName.Returns(string.Empty);
        user.Username.Returns("User");

        var result = user.GetDisplayName();

        Assert.AreEqual("User", result);
    }

    [TestMethod]
    public void GetDisplayName_UserWithGlobalName_ReturnsGlobalName()
    {
        var user = Substitute.For<IUser>();
        user.GlobalName.Returns("Global");
        user.Username.Returns("User");

        var result = user.GetDisplayName();

        Assert.AreEqual("Global", result);
    }
}