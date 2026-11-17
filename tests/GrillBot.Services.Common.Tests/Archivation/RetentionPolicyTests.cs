using GrillBot.Services.Common.Archivation;

namespace GrillBot.Services.Common.Tests.Archivation;

[TestClass]
public class RetentionPolicyTests
{
    private static readonly DateTime Now = new(2026, 11, 17, 2, 0, 0, DateTimeKind.Utc);

    private sealed class Item
    {
        public string Type { get; init; } = "Info";
        public string? Message { get; init; }
        public DateTime CreatedAt { get; init; }
        public bool IsDeleted { get; init; }
    }

    private static RetentionPolicy<Item> CreatePolicy(bool withAlwaysExpired = false)
    {
        RetentionRule<Item>[] rules =
        [
            new("Reconnect", o => o.Type == "Info" && o.Message != null && o.Message.Contains("Reconnect"), RetentionPeriod.FromMonths(6)),
            new("Channels", o => o.Type == "Channel", RetentionPeriod.FromMonths(24)),
            new("AllInfo", o => o.Type == "Info", RetentionPeriod.FromMonths(36))
        ];

        return new RetentionPolicy<Item>(
            o => o.CreatedAt,
            RetentionPeriod.FromMonths(12),
            rules,
            withAlwaysExpired ? o => o.IsDeleted : null
        );
    }

    private static bool IsExpired(RetentionPolicy<Item> policy, Item item)
        => policy.BuildExpiredPredicate(Now).Compile()(item);

    private static DateTime MonthsAgo(int months, int extraDays = 0)
        => Now.AddMonths(-months).AddDays(-extraDays);

    [TestMethod]
    public void ConditionalRule_UsesItsShorterRetention()
    {
        var policy = CreatePolicy();

        Assert.IsTrue(IsExpired(policy, new Item { Message = "GatewayReconnect", CreatedAt = MonthsAgo(7) }));
        Assert.IsFalse(IsExpired(policy, new Item { Message = "GatewayReconnect", CreatedAt = MonthsAgo(5) }));
    }

    [TestMethod]
    public void FirstMatchWins_OverBroaderRuleForSameType()
    {
        var policy = CreatePolicy();

        // Not matching the conditional rule falls through to "AllInfo" (36 months), not to the default.
        Assert.IsFalse(IsExpired(policy, new Item { Message = "Other", CreatedAt = MonthsAgo(13) }));
        Assert.IsFalse(IsExpired(policy, new Item { Message = null, CreatedAt = MonthsAgo(35) }));
        Assert.IsTrue(IsExpired(policy, new Item { Message = "Other", CreatedAt = MonthsAgo(37) }));
    }

    [TestMethod]
    public void UnconditionalRule_ExtendsRetention()
    {
        var policy = CreatePolicy();

        Assert.IsFalse(IsExpired(policy, new Item { Type = "Channel", CreatedAt = MonthsAgo(13) }));
        Assert.IsTrue(IsExpired(policy, new Item { Type = "Channel", CreatedAt = MonthsAgo(24) }));
    }

    [TestMethod]
    public void NoRuleMatches_UsesDefault()
    {
        var policy = CreatePolicy();

        Assert.IsFalse(IsExpired(policy, new Item { Type = "Api", CreatedAt = MonthsAgo(11) }));
        Assert.IsTrue(IsExpired(policy, new Item { Type = "Api", CreatedAt = MonthsAgo(12) }));
    }

    [TestMethod]
    public void AlwaysExpired_IgnoresAge()
    {
        var policy = CreatePolicy(withAlwaysExpired: true);

        Assert.IsTrue(IsExpired(policy, new Item { Type = "Api", IsDeleted = true, CreatedAt = Now }));
        Assert.IsFalse(IsExpired(policy, new Item { Type = "Api", CreatedAt = Now }));
    }

    [TestMethod]
    public void DefaultOnly_WithTimeSpan()
    {
        var policy = new RetentionPolicy<Item>(o => o.CreatedAt, RetentionPeriod.FromTimeSpan(TimeSpan.FromDays(730)));

        Assert.IsTrue(IsExpired(policy, new Item { CreatedAt = Now.AddDays(-731) }));
        Assert.IsFalse(IsExpired(policy, new Item { CreatedAt = Now.AddDays(-729) }));
    }

    [TestMethod]
    public void FindRule_ReturnsFirstMatchOrNull()
    {
        var policy = CreatePolicy();

        Assert.AreEqual("Reconnect", policy.FindRule(new Item { Message = "Reconnect" })?.Name);
        Assert.AreEqual("AllInfo", policy.FindRule(new Item { Message = "x" })?.Name);
        Assert.IsNull(policy.FindRule(new Item { Type = "Api" }));
    }

    [TestMethod]
    public void DuplicateRuleName_Throws()
    {
        RetentionRule<Item>[] rules =
        [
            new("A", o => o.Type == "Info", RetentionPeriod.FromMonths(1)),
            new("A", o => o.Type == "Api", RetentionPeriod.FromMonths(2))
        ];

        Assert.ThrowsExactly<ArgumentException>(() => new RetentionPolicy<Item>(o => o.CreatedAt, RetentionPeriod.FromMonths(12), rules));
    }

    [TestMethod]
    public void RetentionPeriod_RejectsNonPositive()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => RetentionPeriod.FromMonths(0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => RetentionPeriod.FromTimeSpan(TimeSpan.Zero));
    }
}
