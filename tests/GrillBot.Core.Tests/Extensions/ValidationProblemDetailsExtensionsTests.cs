using System.ComponentModel.DataAnnotations;
using GrillBot.Core.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Core.Tests.Extensions;

[TestClass]
public class ValidationProblemDetailsExtensionsTests
{
    [TestMethod]
    public void AggregateAndThrow_Null()
    {
        try
        {
            ((ValidationProblemDetails?)null).AggregateAndThrow();
        }
        catch (Exception ex)
        {
            Assert.Fail($"Expected no exception, but got: {ex}");
        }
    }

    [TestMethod]
    public void AggregateAndThrow_NoErrors()
    {
        var details = new ValidationProblemDetails();
        details.AggregateAndThrow();

        Assert.IsEmpty(details.Errors);
    }

    [TestMethod]
    public void AggregateAndThrow()
    {
        var details = new ValidationProblemDetails();
        details.Errors.Add("Err", ["Error"]);

        Assert.ThrowsExactly<ValidationException>(details.AggregateAndThrow);
    }

    [TestMethod]
    public void ThrowFirstError_Null()
    {
        try
        {
            ((ValidationProblemDetails?)null).ThrowFirstError();
        }
        catch (Exception ex)
        {
            Assert.Fail($"Expected no exception, but got: {ex}");
        }
    }

    [TestMethod]
    public void ThrowFirstError_NoErrors()
    {
        var details = new ValidationProblemDetails();
        details.ThrowFirstError();

        Assert.IsEmpty(details.Errors);
    }

    [TestMethod]
    public void ThrowFirstError()
    {
        var details = new ValidationProblemDetails();
        details.Errors.Add("Err", ["Error"]);

        Assert.ThrowsExactly<ValidationException>(details.ThrowFirstError);
    }
}
