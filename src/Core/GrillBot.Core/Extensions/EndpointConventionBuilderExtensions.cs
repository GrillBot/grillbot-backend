using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GrillBot.Core.Extensions;

public static class EndpointConventionBuilderExtensions
{
    public static TBuilder RequireUserAgent<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(b =>
        {
            var originalDelegate = b.RequestDelegate;

            b.RequestDelegate = async ctx =>
            {
                var userAgent = ctx.Request.Headers.UserAgent.ToString();
                if (string.IsNullOrWhiteSpace(userAgent))
                {
                    ctx.Response.StatusCode = StatusCodes.Status400BadRequest; // Bad Request

                    var accept = ctx.Request.Headers.Accept.ToString();
                    if (accept.Contains("text/plain", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(accept))
                    {
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.WriteAsync("User-Agent header is required.");
                    }
                    else
                    {
                        ctx.Response.ContentType = "application/problem+json";
                        var problem = new ProblemDetails
                        {
                            Status = StatusCodes.Status400BadRequest,
                            Title = "User-Agent header is required.",
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                        };
                        await JsonSerializer.SerializeAsync(ctx.Response.Body, problem, cancellationToken: ctx.RequestAborted);
                    }
                    return;
                }

                if (originalDelegate is not null)
                    await originalDelegate(ctx);
            };
        });

        return builder;
    }
}
