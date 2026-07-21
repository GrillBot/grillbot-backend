using GrillBot.Core.Validation;

namespace GrillBot.Core.Tests.Validation;

[TestClass]
public class RequireTimezoneAttributeTests : ValidationAttributeTestBase<RequireTimezoneAttribute>
{
    protected override RequireTimezoneAttribute CreateAttribute() => new();

    [TestMethod]
    public void NullValue()
        => Assert.IsNull(Attribute.GetValidationResult(null, Context));

    [TestMethod]
    public void InvalidType()
        => Assert.IsNotNull(Attribute.GetValidationResult("Test", Context));

    [TestMethod]
    public void LocalDateTime()
        => Assert.IsNull(Attribute.GetValidationResult(DateTime.Now, Context));

    [TestMethod]
    public void UtcDateTime()
        => Assert.IsNull(Attribute.GetValidationResult(DateTime.UtcNow, Context));

    [TestMethod]
    public void UnspecifiedDateTime()
        => Assert.IsNotNull(Attribute.GetValidationResult(new DateTime(DateTime.Now.Ticks, DateTimeKind.Unspecified), Context));
}
