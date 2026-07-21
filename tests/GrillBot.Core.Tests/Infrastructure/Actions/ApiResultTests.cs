using GrillBot.Core.Infrastructure.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GrillBot.Core.Tests.Infrastructure.Actions;

[TestClass]
public class ApiResultTests
{
    [TestMethod]
    public void Ok()
    {
        var result = ApiResult.Ok();

        Assert.IsNotNull(result);
        Assert.IsNull(result.Data);
        Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
    }

    [TestMethod]
    public void ToApiResult_IActionResult()
    {
        var apiResult = new ApiResult(StatusCodes.Status200OK, new PhysicalFileResult("file.png", "image/png"));
        var result = apiResult.ToApiResult();

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<PhysicalFileResult>(result);
    }

    [TestMethod]
    public void ToApiResult_NullData()
    {
        var apiResult = new ApiResult(StatusCodes.Status200OK);
        var result = apiResult.ToApiResult();

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<StatusCodeResult>(result);
        Assert.AreEqual(StatusCodes.Status200OK, ((StatusCodeResult)result).StatusCode);
    }

    [TestMethod]
    public void ToApiResult_ObjectResult()
    {
        var apiResult = new ApiResult(StatusCodes.Status200OK, "Test");
        var result = apiResult.ToApiResult();

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<ObjectResult>(result);
        Assert.AreEqual(StatusCodes.Status200OK, ((ObjectResult)result).StatusCode);
        Assert.AreEqual("Test", ((ObjectResult)result).Value);
    }

    [TestMethod]
    public void NotFound()
    {
        var result = ApiResult.NotFound();

        Assert.IsNotNull(result);
        Assert.IsNull(result.Data);
        Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
    }

    [TestMethod]
    public void BadRequest()
    {
        var result = ApiResult.BadRequest((ValidationProblemDetails?)null);

        Assert.IsNotNull(result);
        Assert.IsNull(result.Data);
        Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [TestMethod]
    public void BadRequest_ModelState()
    {
        var result = ApiResult.BadRequest((ModelStateDictionary?)null);

        Assert.IsNotNull(result);
        Assert.IsNull(result.Data);
        Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
    }
}
