using GrillBot.Core.Validation;
using GrillBot.Services.Common.Infrastructure.Api.Filters;
using GrillBot.Services.Common.Registrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GrillBot.Services.Common.Tests.Infrastructure.Api.Filters;

public class SampleRequest
{
    public string? Name { get; set; }
}

public class UnvalidatedRequest
{
    public string? Name { get; set; }
}

public class SampleRequestValidator : ModelValidator<SampleRequest>
{
    protected override IEnumerable<Func<SampleRequest, ValidationContext, IEnumerable<ValidationResult>>> GetValidations()
    {
        yield return (model, _) => string.IsNullOrEmpty(model.Name)
            ? [new ValidationResult("Name/Required", [nameof(SampleRequest.Name)])]
            : [];
    }
}

[TestClass]
public class ModelValidationFilterTests
{
    [TestMethod]
    public void Order_RunsBeforeApiControllerModelStateFilter()
    {
        // [ApiController]'s automatic 400 filter sits at -2000. Anything this filter adds to
        // ModelState is only turned into a response if it lands first.
        Assert.IsLessThan(-2000, new ModelValidationFilter().Order);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_FailingValidator_AddsModelErrorUnderMemberName()
    {
        var context = CreateContext(new SampleRequest { Name = null });

        var wasCalled = false;
        await new ModelValidationFilter().OnActionExecutionAsync(context, () =>
        {
            wasCalled = true;
            return Task.FromResult(CreateExecutedContext(context));
        });

        Assert.IsTrue(wasCalled);
        Assert.IsFalse(context.ModelState.IsValid);
        Assert.AreEqual("Name/Required", context.ModelState["Name"]!.Errors[0].ErrorMessage);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_PassingValidator_KeepsModelStateValid()
    {
        var context = CreateContext(new SampleRequest { Name = "value" });

        await new ModelValidationFilter().OnActionExecutionAsync(context, () => Task.FromResult(CreateExecutedContext(context)));

        Assert.IsTrue(context.ModelState.IsValid);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_ArgumentWithoutValidator_IsIgnored()
    {
        var context = CreateContext(new UnvalidatedRequest { Name = null });

        await new ModelValidationFilter().OnActionExecutionAsync(context, () => Task.FromResult(CreateExecutedContext(context)));

        Assert.IsTrue(context.ModelState.IsValid);
    }

    [TestMethod]
    public void RegisterValidatorsFromAssembly_RegistersUnderClosedGenericBase()
    {
        var services = new ServiceCollection();
        services.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var validator = scope.ServiceProvider.GetService<ModelValidator<SampleRequest>>();

        Assert.IsNotNull(validator);
        Assert.IsInstanceOfType<SampleRequestValidator>(validator);
    }

    private static ActionExecutingContext CreateContext(object argument)
    {
        var services = new ServiceCollection();
        services.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor(), new ModelStateDictionary());

        return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?> { { "request", argument } }, controller: null!);
    }

    private static ActionExecutedContext CreateExecutedContext(ActionExecutingContext context)
        => new(context, context.Filters, context.Controller);
}
