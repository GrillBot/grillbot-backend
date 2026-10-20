using GrillBot.App.Actions;
using GrillBot.App.Actions.Api;
using GrillBot.App.Actions.Api.V2.User;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Contracts.UserManagement.Requests;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Data.Models.API.Infractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagementService;

namespace GrillBot.App.Controllers;

[ApiKeyAuth]
[Route("api/user/infractions")]
[ApiExplorerSettings(GroupName = "v2")]
public class InfractionsController(IServiceProvider serviceProvider) : Core.Infrastructure.Actions.ControllerBase(serviceProvider)
{
    /// <summary>
    /// Create or update member timeout.
    /// </summary>
    /// <param name="parameters">Timeout parameters</param>
    /// <response code="201">Timeout has been successfully created.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpPost("timeout")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTimeoutAsync([FromBody] CreateInfractionTimeoutParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<CreateInfractionTimeout>(parameters);
    }

    /// <summary>
    /// Delete member timeout.
    /// </summary>
    /// <param name="timeoutId">Timeout ID from external system.</param>
    /// <response code="200">Timeout has been successfully deleted.</response>
    /// <response code="400">Validation of parameters failed.</response>
    /// <response code="404">Timeout wasn't found.</response>
    [HttpDelete("timeout/{timeoutId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTimeoutAsync(long timeoutId)
    {
        var executor = new Func<IUserManagementServiceClient, ServiceExecutorContext, Task>(
            (client, ctx) => client.DeleteInfractionAsync(DeleteInfractionRequest.FromExternalSystem(timeoutId, "Timeout"), ctx.CancellationToken)
        );

        return await ProcessAsync<ServiceBridgeAction<IUserManagementServiceClient>>(executor);
    }
}
