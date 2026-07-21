using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GrillBot.Core.HealthCheck.Tests;

[TestClass]
public class HealthCheckWriterTests
{
    [TestMethod]
    public async Task WriteResponseAsync_UnhealthyStatus_Returns503()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Unhealthy, TimeSpan.Zero);

        // Act
        var result = await HealthCheckWriter.WriteResponseAsync(context, report);

        // Assert
        Assert.AreEqual(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        Assert.AreEqual("application/json", context.Response.ContentType);
        Assert.IsInstanceOfType<EmptyResult>(result);
    }

    [TestMethod]
    [DataRow(HealthStatus.Healthy)]
    [DataRow(HealthStatus.Degraded)]
    public async Task WriteResponseAsync_HealthyOrDegradedStatus_Returns200(HealthStatus status)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>(), status, TimeSpan.Zero);

        // Act
        var result = await HealthCheckWriter.WriteResponseAsync(context, report);

        // Assert
        Assert.AreEqual(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.AreEqual("application/json", context.Response.ContentType);
        Assert.IsInstanceOfType<EmptyResult>(result);
    }
}