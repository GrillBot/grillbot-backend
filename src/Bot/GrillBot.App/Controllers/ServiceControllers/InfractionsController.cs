using GrillBot.App.Actions.Api.V3.Services.Infractions;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Contracts.UserManagement.Requests;
using GrillBot.Contracts.UserManagement.Responses;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Infractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagementService;

namespace GrillBot.App.Controllers.ServiceControllers;

[JwtAuthorize("Infractions(Admin)")]
public class InfractionsController(IServiceProvider serviceProvider) : ServiceControllerBase<IUserManagementServiceClient>(serviceProvider)
{
    [HttpPost("list")]
    [ProducesResponseType(typeof(PaginatedResponse<InfractionItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetInfractionsListAsync([FromBody] InfractionListParams parameters)
        => ExecuteAsync(async (client, ctx) => await client.GetInfractionsListAsync(parameters, ctx.CancellationToken), parameters);

    [HttpDelete("{infractionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    public Task<IActionResult> DeleteInfractionAsync([FromRoute] Guid infractionId)
        => ExecuteAsync(async (client, ctx) => await client.DeleteInfractionAsync(DeleteInfractionRequest.FromInternalId(infractionId), ctx.CancellationToken));

    [HttpPost("member-warning")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> CreateMemberWarningAsync([FromBody] CreateMemberWarningParams parameters)
        => ExecuteAsync<CreateMemberWarningAction>(parameters);

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(List<InfractionDashboardRow>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetDashboard()
        => ExecuteAsync(async (client, ctx) => await client.GetInfractionsDashboardAsync(ctx.CancellationToken));
}
