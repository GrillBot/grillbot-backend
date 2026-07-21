using GrillBot.Core.Services.Diagnostics.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace GrillBot.Core.Services.Diagnostics;

public class DiagnosticsManager
{
    public List<RequestStatistics> Statistics { get; } = [];

    private readonly SemaphoreSlim _semaphore = new(1);

    public async Task OnRequestEndAsync(ActionExecutingContext context, ActionExecutedContext result, DateTime startAt)
    {
        var now = DateTime.Now;
        var endpoint = $"{context.HttpContext.Request.Method} {context.ActionDescriptor.AttributeRouteInfo!.Template}";
        var duration = Convert.ToInt32((now - startAt).TotalMilliseconds);
        var success = result.Exception is null && result.Result is IStatusCodeActionResult { StatusCode: >= 200 };

        await _semaphore.WaitAsync(context.HttpContext.RequestAborted);
        try
        {
            var statistics = Statistics.Find(o => o.Endpoint == endpoint);
            if (statistics is null)
            {
                statistics = new RequestStatistics { Endpoint = endpoint };
                Statistics.Add(statistics);
            }

            if (success)
                statistics.SuccessCount++;
            else
                statistics.FailedCount++;

            statistics.LastRequestAt = DateTime.Now;
            statistics.LastTime = duration;
            statistics.TotalTime += duration;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
