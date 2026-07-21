using GrillBot.Core.Database.ValueObjects;

namespace GrillBot.Core.Tests.Database.ValueObjects;

[TestClass]
public class DiscordIdValueObjectTests
{
    private const ulong ID = 423559098647183361;

    [TestMethod]
    public void ImplicitOperators_UlongTypes()
    {
        var valueObject = (DiscordIdValueObject)ID;
        var reverseValue = (ulong)valueObject;

        Assert.AreEqual(ID, reverseValue);
    }

    [TestMethod]
    public void ImplicitOperators_DateTimeOffset()
    {
        var value = DateTimeOffset.UtcNow;

        var valueObject = (DiscordIdValueObject)value;
        var reverseValue = (DateTimeOffset)valueObject;

        Assert.AreEqual(value.ToUnixTimeMilliseconds(), reverseValue.ToUnixTimeMilliseconds());
    }

    [TestMethod]
    public void ValueObject_ToString()
    {
        var expected = ID.ToString();

        var valueObject = new DiscordIdValueObject(ID);
        var result = valueObject.ToString();

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void CompareTo()
    {
        var valueObject = new DiscordIdValueObject(ID);
        Assert.AreEqual(0, valueObject.CompareTo(ID));
    }

    [TestMethod]
    public void Equals_AsObject()
    {
        var valueObject = new DiscordIdValueObject(ID);
        Assert.IsTrue(valueObject.Equals(valueObject));
    }

    [TestMethod]
    public void Operators()
    {
        var valueObject1 = new DiscordIdValueObject(ID);
        var valueObject2 = new DiscordIdValueObject(ID);

        Assert.IsTrue(valueObject1 == valueObject2);
        Assert.IsFalse(valueObject1 != valueObject2);
        Assert.IsFalse(valueObject1 < valueObject2);
        Assert.IsTrue(valueObject1 <= valueObject2);
        Assert.IsFalse(valueObject1 > valueObject2);
        Assert.IsTrue(valueObject1 >= valueObject2);
    }
}
