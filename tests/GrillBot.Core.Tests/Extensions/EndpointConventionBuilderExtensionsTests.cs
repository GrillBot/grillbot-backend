using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using GrillBot.Core.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GrillBot.Core.Tests.Extensions;

[TestClass]
public class EndpointConventionBuilderExtensionsTests
{
    public TestContext TestContext { get; set; }

    private class TestEndpointConventionBuilder : IEndpointConventionBuilder
    {
        public Action<EndpointBuilder>? CapturedAction { get; private set; }

        public void Add(Action<EndpointBuilder> convention)
        {
            CapturedAction = convention;
        }
    }

    [TestMethod]
    public async Task RequireUserAgent_WithUserAgentHeader_CallsOriginalDelegate()
    {
        var builder = new TestEndpointConventionBuilder();
        var originalDelegateCalled = false;

        var endpointBuilder = new RouteEndpointBuilder(_ =>
        {
            originalDelegateCalled = true;
            return Task.CompletedTask;
        }, RoutePatternFactory.Parse("/"), 0);

        builder.RequireUserAgent();
        builder.CapturedAction!(endpointBuilder);

        var context = new DefaultHttpContext();
        context.Request.Headers.UserAgent = "TestAgent";
        context.Response.Body = new MemoryStream();

        await endpointBuilder.RequestDelegate!(context);

        Assert.IsTrue(originalDelegateCalled);
        Assert.AreEqual(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [TestMethod]
    public async Task RequireUserAgent_WithoutUserAgentHeader_ReturnsBadRequest_PlainText()
    {
        var builder = new TestEndpointConventionBuilder();
        var originalDelegateCalled = false;

        var endpointBuilder = new RouteEndpointBuilder(_ =>
        {
            originalDelegateCalled = true;
            return Task.CompletedTask;
        }, RoutePatternFactory.Parse("/"), 0);

        builder.RequireUserAgent();
        builder.CapturedAction!(endpointBuilder);

        var context = new DefaultHttpContext();
        context.Request.Headers.Accept = "text/plain";
        context.Response.Body = new MemoryStream();

        await endpointBuilder.RequestDelegate!(context);

        Assert.IsFalse(originalDelegateCalled);
        Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.AreEqual("text/plain", context.Response.ContentType);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
        var responseText = await reader.ReadToEndAsync(TestContext.CancellationToken);
        Assert.AreEqual("User-Agent header is required.", responseText);
    }

    [TestMethod]
    public async Task RequireUserAgent_WithoutUserAgentHeader_ReturnsBadRequest_ProblemDetails()
    {
        var builder = new TestEndpointConventionBuilder();
        var originalDelegateCalled = false;

        var endpointBuilder = new RouteEndpointBuilder(_ =>
        {
            originalDelegateCalled = true;
            return Task.CompletedTask;
        }, RoutePatternFactory.Parse("/"), 0);

        builder.RequireUserAgent();
        builder.CapturedAction!(endpointBuilder);

        var context = new DefaultHttpContext();
        context.Request.Headers.Accept = "application/json";
        context.Response.Body = new MemoryStream();

        await endpointBuilder.RequestDelegate!(context);

        Assert.IsFalse(originalDelegateCalled);
        Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.AreEqual("application/problem+json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
        var responseText = await reader.ReadToEndAsync(TestContext.CancellationToken);

        var problem = JsonSerializer.Deserialize<ProblemDetails>(responseText);
        Assert.IsNotNull(problem);
        Assert.AreEqual(StatusCodes.Status400BadRequest, problem.Status);
        Assert.AreEqual("User-Agent header is required.", problem.Title);
        Assert.AreEqual("https://tools.ietf.org/html/rfc7231#section-6.5.1", problem.Type);
    }

    [TestMethod]
    public async Task RequireUserAgent_WithEmptyUserAgentHeader_ReturnsBadRequest_PlainText()
    {
        var builder = new TestEndpointConventionBuilder();
        var originalDelegateCalled = false;

        var endpointBuilder = new RouteEndpointBuilder(_ =>
        {
            originalDelegateCalled = true;
            return Task.CompletedTask;
        }, RoutePatternFactory.Parse("/"), 0);

        builder.RequireUserAgent();
        builder.CapturedAction!(endpointBuilder);

        var context = new DefaultHttpContext();
        context.Request.Headers.UserAgent = "";
        context.Request.Headers.Accept = "text/plain";
        context.Response.Body = new MemoryStream();

        await endpointBuilder.RequestDelegate!(context);

        Assert.IsFalse(originalDelegateCalled);
        Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.AreEqual("text/plain", context.Response.ContentType);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
        var responseText = await reader.ReadToEndAsync(TestContext.CancellationToken);
        Assert.AreEqual("User-Agent header is required.", responseText);
    }

    [TestMethod]
    public async Task RequireUserAgent_WithEmptyUserAgentHeader_ReturnsBadRequest_ProblemDetails()
    {
        var builder = new TestEndpointConventionBuilder();
        var originalDelegateCalled = false;

        var endpointBuilder = new RouteEndpointBuilder(_ =>
        {
            originalDelegateCalled = true;
            return Task.CompletedTask;
        }, RoutePatternFactory.Parse("/"), 0);

        builder.RequireUserAgent();
        builder.CapturedAction!(endpointBuilder);

        var context = new DefaultHttpContext();
        context.Request.Headers.UserAgent = "";
        context.Request.Headers.Accept = "application/json";
        context.Response.Body = new MemoryStream();

        await endpointBuilder.RequestDelegate!(context);

        Assert.IsFalse(originalDelegateCalled);
        Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.AreEqual("application/problem+json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
        var responseText = await reader.ReadToEndAsync(TestContext.CancellationToken);

        var problem = JsonSerializer.Deserialize<ProblemDetails>(responseText);
        Assert.IsNotNull(problem);
        Assert.AreEqual(StatusCodes.Status400BadRequest, problem.Status);
        Assert.AreEqual("User-Agent header is required.", problem.Title);
        Assert.AreEqual("https://tools.ietf.org/html/rfc7231#section-6.5.1", problem.Type);
    }
}