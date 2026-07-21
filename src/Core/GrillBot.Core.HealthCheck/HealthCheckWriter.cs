using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GrillBot.Core.HealthCheck;

public static class HealthCheckWriter
{
    public static async Task<IActionResult> WriteResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.StatusCode = report.Status switch
        {
            HealthStatus.Unhealthy => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status200OK
        };

        context.Response.ContentType = "application/json";

        await UIResponseWriter.WriteHealthCheckUIResponse(context, report);
        return new EmptyResult();
    }
}
