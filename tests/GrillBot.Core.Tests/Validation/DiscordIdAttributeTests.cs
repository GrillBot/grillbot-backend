using GrillBot.Core.Validation;

namespace GrillBot.Core.Tests.Validation;

[TestClass]
public class DiscordIdAttributeTests : ValidationAttributeTestBase<DiscordIdAttribute>
{
    protected override DiscordIdAttribute CreateAttribute() => new();

    [TestMethod]
    public void UnknownIdentifier()
    {
        var result = Attribute.GetValidationResult(new object(), Context);

        Assert.IsNotNull(result);
        Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
        Assert.Contains("Unsupported type", result.ErrorMessage);
    }

    [TestMethod]
    public void NullIdentifier()
        => Assert.IsNull(Attribute.GetValidationResult(null, Context));

    [TestMethod]
    public void ValidId()
        => Assert.IsNull(Attribute.GetValidationResult(1111355832228126871UL, Context));

    [TestMethod]
    public void ZeroId()
        => Assert.IsNotNull(Attribute.GetValidationResult("0", Context));

    [TestMethod]
    public void InvalidId_StringChars()
        => Assert.IsNotNull(Attribute.GetValidationResult(new List<string> { "FJDSIJFO" }, Context));

    [TestMethod]
    public void InvalidId2()
        => Assert.IsNotNull(Attribute.GetValidationResult(new List<ulong> { 1UL }, Context));

    [TestMethod]
    public void ValidCollection()
        => Assert.IsNull(Attribute.GetValidationResult(new List<ulong> { 1111355832228126871UL, 1111355832228126871UL }, Context));
}
