using GrillBot.Contracts.UserManagement.Requests;
using GrillBot.Contracts.UserManagement.Responses;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using Microsoft.AspNetCore.Mvc;
using UserManagementService.Actions.Infractions;
using ControllerBase = GrillBot.Core.Infrastructure.Actions.ControllerBase;

namespace UserManagementService.Controllers;

[Route("api/infractions")]
public class InfractionsController(IServiceProvider serviceProvider) : ControllerBase(serviceProvider)
{
    [HttpGet("dashboard")]
    [ProducesResponseType<List<InfractionDashboardRow>>(StatusCodes.Status200OK)]
    public Task<IActionResult> GetDashboardAsync()
        => ProcessAsync<GetInfractionsDashboard>();

    [HttpGet("count/{guildId}")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    public Task<IActionResult> GetCountOfGuildAsync([DiscordId] string guildId)
        => ProcessAsync<GetInfractionsCountOfGuild>(guildId);

    [HttpPost("list")]
    [ProducesResponseType<PaginatedResponse<InfractionItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetInfractionsListAsync(InfractionListParams parameters)
        => ProcessAsync<GetInfractionsList>(parameters);

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> DeleteInfractionAsync([FromQuery] DeleteInfractionRequest request)
        => ProcessAsync<DeleteInfractionAction>(request);

    [HttpGet("user/{userId}")]
    [ProducesResponseType<UserInfractionsSummary>(StatusCodes.Status200OK)]
    public Task<IActionResult> GetUserSummaryAsync(
        [FromRoute, DiscordId] string userId,
        [FromQuery, DiscordId] string? guildId
    ) => ProcessAsync<GetUserInfractionsSummary>(userId, guildId);
}
