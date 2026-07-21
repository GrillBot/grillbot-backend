using Discord;
using GrillBot.Core.Extensions.Discord;
using NSubstitute;

namespace GrillBot.Core.Tests.Extensions.Discord;

[TestClass]
public class GuildUserExtensionsTests
{
    [TestMethod]
    public void GetRoles_WithoutEveryoneRole_FiltersEveryoneRole()
    {
        // Arrange
        var everyoneRole = Substitute.For<IRole>();
        everyoneRole.Id.Returns(1UL);

        var role2 = Substitute.For<IRole>();
        role2.Id.Returns(2UL);

        var role3 = Substitute.For<IRole>();
        role3.Id.Returns(3UL);

        var guild = Substitute.For<IGuild>();
        guild.EveryoneRole.Returns(everyoneRole);
        guild.GetRole(1UL).Returns(everyoneRole);
        guild.GetRole(2UL).Returns(role2);
        guild.GetRole(3UL).Returns(role3);

        var user = Substitute.For<IGuildUser>();
        user.Guild.Returns(guild);
        user.RoleIds.Returns([1UL, 2UL, 3UL]);

        // Act
        var roles = user.GetRoles().ToList();

        // Assert
        Assert.HasCount(2, roles);
        Assert.IsFalse(roles.Any(r => r.Id == 1UL));
        Assert.IsTrue(roles.Any(r => r.Id == 2UL));
        Assert.IsTrue(roles.Any(r => r.Id == 3UL));
    }

    [TestMethod]
    public void GetRoles_WithEveryoneRole_IncludesEveryoneRole()
    {
        // Arrange
        var everyoneRole = Substitute.For<IRole>();
        everyoneRole.Id.Returns(1UL);

        var role2 = Substitute.For<IRole>();
        role2.Id.Returns(2UL);

        var guild = Substitute.For<IGuild>();
        guild.EveryoneRole.Returns(everyoneRole);
        guild.GetRole(1UL).Returns(everyoneRole);
        guild.GetRole(2UL).Returns(role2);

        var user = Substitute.For<IGuildUser>();
        user.Guild.Returns(guild);
        user.RoleIds.Returns([1UL, 2UL]);

        // Act
        var roles = user.GetRoles(withEveryone: true).ToList();

        // Assert
        Assert.HasCount(2, roles);
        Assert.IsTrue(roles.Any(r => r.Id == 1UL));
        Assert.IsTrue(roles.Any(r => r.Id == 2UL));
    }

    [TestMethod]
    public void GetRoles_EmptyRoleIds_ReturnsEmpty()
    {
        // Arrange
        var everyoneRole = Substitute.For<IRole>();
        everyoneRole.Id.Returns(1UL);

        var guild = Substitute.For<IGuild>();
        guild.EveryoneRole.Returns(everyoneRole);

        var user = Substitute.For<IGuildUser>();
        user.Guild.Returns(guild);
        user.RoleIds.Returns([]);

        // Act
        var roles = user.GetRoles().ToList();

        // Assert
        Assert.IsEmpty(roles);
    }

    [TestMethod]
    public void GetRoles_NonExistentRoleIds_SkipsNullRoles()
    {
        // Arrange
        var everyoneRole = Substitute.For<IRole>();
        everyoneRole.Id.Returns(1UL);

        var role2 = Substitute.For<IRole>();
        role2.Id.Returns(2UL);

        var guild = Substitute.For<IGuild>();
        guild.EveryoneRole.Returns(everyoneRole);
        guild.GetRole(1UL).Returns(everyoneRole);
        guild.GetRole(2UL).Returns(role2);
        guild.GetRole(3UL).Returns((IRole)null!); // Simulate missing role

        var user = Substitute.For<IGuildUser>();
        user.Guild.Returns(guild);
        user.RoleIds.Returns([1UL, 2UL, 3UL]);

        // Act
        var roles = user.GetRoles(withEveryone: true).ToList();

        // Assert
        Assert.HasCount(2, roles);
        Assert.IsTrue(roles.Any(r => r.Id == 1UL));
        Assert.IsTrue(roles.Any(r => r.Id == 2UL));
        Assert.Contains(r => r != null, roles);
    }
}