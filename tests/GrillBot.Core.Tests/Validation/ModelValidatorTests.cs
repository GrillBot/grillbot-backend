using GrillBot.Core.Tests.Validation.ModelValidator;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Core.Tests.Validation;

[TestClass]
public class ModelValidatorTests
{
    [TestMethod]
    public void NoValue()
    {
        var result = ProcessTest(new DataModelClass());
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void AllFailed()
    {
        var data = new DataModelClass
        {
            Value2 = DateTime.Now.AddDays(1),
            Value3 = DateTime.Now,
            Value4 = 50,
            Value5 = 5
        };

        var result = ProcessTest(data);
        Assert.HasCount(5, result);
    }

    [TestMethod]
    public void AllSuccess()
    {
        var data = new DataModelClass
        {
            Value1 = "Test",
            Value2 = DateTime.UtcNow.AddDays(-1),
            Value3 = DateTime.UtcNow,
            Value4 = 5,
            Value5 = 50
        };

        var result = ProcessTest(data);
        Assert.IsEmpty(result);
    }

    private List<ValidationResult> ProcessTest(DataModelClass data)
    {
        var context = new ValidationContext(this) { MemberName = "Test" };
        var validator = new ClassForModelValidation();
        var result = validator.Validate(data, context);

        Assert.IsNotNull(result);
        return [.. result];
    }
}
