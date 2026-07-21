using GrillBot.Core.Services.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GrillBot.Core.Infrastructure;

public class RequestFilter(DiagnosticsManager _diagnosticsManager, IEnumerable<IRequestFilterAction> _filters) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var startAt = DateTime.Now;

        SetXssProtection(context.HttpContext.Response);
        foreach (var filter in _filters)
            await filter.BeforeExecutionAsync(context.HttpContext.RequestAborted);

        var result = await next();

        foreach (var filter in _filters)
            await filter.AfterExecutionAsync(context.HttpContext.RequestAborted);

        await _diagnosticsManager.OnRequestEndAsync(context, result, startAt);
    }

    private static void SetXssProtection(HttpResponse response)
    {
        response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
        response.Headers.Append("X-Xss-Protection", "1; mode=block");
        response.Headers.Append("X-Content-Type-Options", "nosniff");
    }
}
