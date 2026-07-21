using GrillBot.Core.Infrastructure;
using GrillBot.Core.Services.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace GrillBot.Core.Tests.Infrastructure;

[TestClass]
public class CoreRequestFilterXssTests
{
    [TestMethod]
    public async Task SetXssProtection()
    {
        var filter = new RequestFilter(new DiagnosticsManager(), []);
        var context = CreateContext();

        await filter.OnActionExecutionAsync(context, () => Task.FromResult(CreateResult(context)));

        Assert.IsTrue(context.HttpContext.Response.Headers.Any());
        Assert.IsTrue(context.HttpContext.Response.Headers.ContainsKey("X-Frame-Options"));
        Assert.IsTrue(context.HttpContext.Response.Headers.ContainsKey("X-Xss-Protection"));
        Assert.IsTrue(context.HttpContext.Response.Headers.ContainsKey("X-Content-Type-Options"));
    }

    private static ActionExecutingContext CreateContext()
    {
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        var descriptor = new ControllerActionDescriptor
        {
            AttributeRouteInfo = new AttributeRouteInfo()
        };

        var actionContext = new ActionContext(httpContext, routeData, descriptor);

        return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), null!);
    }

    private static ActionExecutedContext CreateResult(ActionContext context)
        => new(context, [], null!);
}
