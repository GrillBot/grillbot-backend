using GrillBot.Contracts.Bot;
using GrillBot.Contracts.Bot.Events.Messages.Embeds;

namespace GrillBot.Contracts.Tests.Bot.Events.Messages.Embeds;

[TestClass]
public class DiscordMessageEmbedTests
{
    [TestMethod]
    public void ToBuilder_AllPropertiesSet_MapsCorrectly()
    {
        var embed = new DiscordMessageEmbed
        {
            Url = new LocalizedMessageContent("https://test.com", []),
            Title = new LocalizedMessageContent("Test Title", []),
            Description = new LocalizedMessageContent("Test Description", []),
            Author = new DiscordMessageEmbedAuthor("Author Name", "https://author.com", "https://icon.com"),
            Color = 0xFF00FF,
            Footer = new DiscordMessageEmbedFooter("Footer Text", "https://footericon.com"),
            ImageUrl = new LocalizedMessageContent("https://image.com", []),
            ThumbnailUrl = new LocalizedMessageContent("https://thumb.com", []),
            Fields =
            [
                new("Field1", "Value1", true),
                new("Field2", "Value2", false)
            ],
            Timestamp = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            UseCurrentTimestamp = false
        };

        var builder = embed.ToBuilder();
        var built = builder.Build();

        Assert.AreEqual("https://test.com", built.Url);
        Assert.AreEqual("Test Title", built.Title);
        Assert.AreEqual("Test Description", built.Description);
        Assert.AreEqual("Author Name", built.Author?.Name);
        Assert.AreEqual("https://author.com", built.Author?.Url);
        Assert.AreEqual("https://icon.com", built.Author?.IconUrl);
        Assert.IsNotNull(built.Color?.RawValue);
        Assert.AreEqual((uint)0xFF00FF, built.Color.Value.RawValue);
        Assert.AreEqual("Footer Text", built.Footer?.Text);
        Assert.AreEqual("https://footericon.com", built.Footer?.IconUrl);
        Assert.AreEqual("https://image.com", built.Image?.Url);
        Assert.AreEqual("https://thumb.com", built.Thumbnail?.Url);
        Assert.HasCount(2, built.Fields);
        Assert.AreEqual("Field1", built.Fields[0].Name);
        Assert.AreEqual("Value1", built.Fields[0].Value);
        Assert.IsTrue(built.Fields[0].Inline);
        Assert.AreEqual("Field2", built.Fields[1].Name);
        Assert.AreEqual("Value2", built.Fields[1].Value);
        Assert.IsFalse(built.Fields[1].Inline);

        Assert.AreEqual(new DateTimeOffset(new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)), built.Timestamp);
    }

    [TestMethod]
    public void ToBuilder_OnlyRequiredPropertiesSet_MapsCorrectly()
    {
        var embed = new DiscordMessageEmbed();

        var builder = embed.ToBuilder();
        var built = builder.Build();

        Assert.IsNull(built.Url);
        Assert.IsNull(built.Title);
        Assert.IsNull(built.Description);
        Assert.IsNull(built.Author);
        Assert.IsNull(built.Color);
        Assert.IsNull(built.Footer);
        Assert.IsNull(built.Image);
        Assert.IsNull(built.Thumbnail);
        Assert.IsEmpty(built.Fields);
        Assert.IsNull(built.Timestamp);
    }

    [TestMethod]
    public void ToBuilder_UseCurrentTimestamp_SetsCurrentTimestamp()
    {
        var embed = new DiscordMessageEmbed
        {
            UseCurrentTimestamp = true
        };

        var builder = embed.ToBuilder();
        var built = builder.Build();

        Assert.IsTrue(built.Timestamp.HasValue);
        Assert.IsLessThan(5, (DateTimeOffset.UtcNow - built.Timestamp.Value).TotalSeconds);
    }

    [TestMethod]
    public void ToBuilder_NullAndEmptyFields_AreIgnored()
    {
        var embed = new DiscordMessageEmbed
        {
            Url = null,
            Title = new LocalizedMessageContent("", []),
            Description = null,
            Author = null,
            Color = null,
            Footer = null,
            ImageUrl = new LocalizedMessageContent("", []),
            ThumbnailUrl = null,
            Fields = [],
            Timestamp = null,
            UseCurrentTimestamp = false
        };

        var builder = embed.ToBuilder();
        var built = builder.Build();

        Assert.IsNull(built.Url);
        Assert.IsNull(built.Title);
        Assert.IsNull(built.Description);
        Assert.IsNull(built.Author);
        Assert.IsNull(built.Color);
        Assert.IsNull(built.Footer);
        Assert.IsNull(built.Image);
        Assert.IsNull(built.Thumbnail);
        Assert.IsEmpty(built.Fields);
        Assert.IsNull(built.Timestamp);
    }
}