using GrillBot.Core.Validation;

namespace GrillBot.Core.Tests.Validation;

[TestClass]
public class RequireSomeItemInCollectionAttributeTests : ValidationAttributeTestBase<RequireSomeItemInCollectionAttribute>
{
    protected override RequireSomeItemInCollectionAttribute CreateAttribute() => new();

    [TestMethod]
    public void NullValue()
        => Assert.IsNull(Attribute.GetValidationResult(null, Context));

    [TestMethod]
    public void StringValue()
        => Assert.IsNull(Attribute.GetValidationResult("Test", Context));

    [TestMethod]
    public void NotEnumerable()
        => Assert.IsNotNull(Attribute.GetValidationResult(12345, Context));

    [TestMethod]
    public void NotContainsAnyItemInArray()
        => Assert.IsNotNull(Attribute.GetValidationResult(Array.Empty<string>(), Context));

    [TestMethod]
    public void NotContainsAnyItemInList()
        => Assert.IsNotNull(Attribute.GetValidationResult(new List<string>(), Context));

    [TestMethod]
    public void NotContainsAnyItemInDictionary()
        => Assert.IsNotNull(Attribute.GetValidationResult(new Dictionary<string, string>(), Context));

    [TestMethod]
    public void ContainsItem()
        => Assert.IsNull(Attribute.GetValidationResult(new List<string> { "A" }, Context));
}
