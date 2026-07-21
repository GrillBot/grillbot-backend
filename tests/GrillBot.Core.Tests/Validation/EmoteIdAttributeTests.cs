using GrillBot.Core.Validation;

namespace GrillBot.Core.Tests.Validation;

[TestClass]
public class EmoteIdAttributeTests : ValidationAttributeTestBase<EmoteIdAttribute>
{
    protected override EmoteIdAttribute CreateAttribute() => new();

    [TestMethod]
    public void NotString()
        => Assert.IsNull(Attribute.GetValidationResult(12345, Context));

    [TestMethod]
    public void InvalidEmote()
        => Assert.IsNotNull(Attribute.GetValidationResult(":emote:", Context));

    [TestMethod]
    public void Success()
        => Assert.IsNull(Attribute.GetValidationResult("<a:test:12345>", Context));
}
