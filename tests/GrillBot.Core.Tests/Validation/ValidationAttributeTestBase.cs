using System.ComponentModel.DataAnnotations;

namespace GrillBot.Core.Tests.Validation;

public abstract class ValidationAttributeTestBase<TAttribute> where TAttribute : ValidationAttribute
{
    protected TAttribute Attribute { get; set; } = null!;

    protected ValidationContext Context { get; private set; } = null!;

    protected abstract TAttribute CreateAttribute();

    [TestInitialize]
    public void Initialize()
    {
        Attribute = CreateAttribute();
        Context = new ValidationContext(this) { MemberName = "Test" };
    }
}
