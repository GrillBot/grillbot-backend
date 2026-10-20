using GrillBot.App.Actions;
using GrillBot.App.Actions.Api;
using GrillBot.App.Actions.Api.V2.User;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Contracts.UserManagement.Requests;
using GrillBot.Data.Models.API.UserMeasures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagementService;

namespace GrillBot.App.Controllers;

/// <summary>
/// Legacy API V2 endpoints, kept unchanged for existing API clients. Use <see cref="InfractionsController"/> instead.
/// The class and method names are part of the contract: ApiClients.AllowedMethods stores them.
/// </summary>
[ApiKeyAuth]
[Route("api/user/measures")]
[ApiExplorerSettings(GroupName = "v2")]
public class UserMeasuresController : Core.Infrastructure.Actions.ControllerBase
{
    public UserMeasuresController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Create or update member timeout.
    /// </summary>
    /// <param name="parameters">Timeout parameters</param>
    /// <response code="200">Timeout has been successfully created.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [Obsolete("Use POST api/user/infractions/timeout.")]
    [HttpPost("timeout/create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUserMeasuresTimeoutAsync([FromBody] CreateUserMeasuresTimeoutParams parameters)
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
    [Obsolete("Use DELETE api/user/infractions/timeout/{timeoutId}.")]
    [HttpDelete("timeout/{timeoutId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUserMeasureTimeoutAsync(long timeoutId)
    {
        var executor = new Func<IUserManagementServiceClient, ServiceExecutorContext, Task>(
            (client, ctx) => client.DeleteInfractionAsync(DeleteInfractionRequest.FromExternalSystem(timeoutId, "Timeout"), ctx.CancellationToken)
        );

        return await ProcessAsync<ServiceBridgeAction<IUserManagementServiceClient>>(executor);
    }
}
