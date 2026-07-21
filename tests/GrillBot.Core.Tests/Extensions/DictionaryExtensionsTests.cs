using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;

namespace GrillBot.Core.Tests.Extensions;

[TestClass]
public class DictionaryExtensionsTests
{
    [TestMethod]
    public void MergeDictionaryObjects()
    {
        var dict = new Dictionary<string, string?>
        {
            { "A", "B" }
        };

        var dictionaryObject = new DictionaryObject<string, string>()
        {
            {  "B", "C" }
        };

        dict.MergeDictionaryObjects(dictionaryObject, "Child");

        Assert.HasCount(2, dict);
        Assert.AreEqual("A", dict.Keys.First());
        Assert.AreEqual("Child.B", dict.Keys.ElementAt(1));
    }
}
