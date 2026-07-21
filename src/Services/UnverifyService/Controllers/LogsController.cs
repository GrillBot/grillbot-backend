using GrillBot.Core.Models.Pagination;
using Microsoft.AspNetCore.Mvc;
using UnverifyService.Actions.Archivation;
using UnverifyService.Actions.Logs;
using GrillBot.Contracts.Unverify.Requests.Logs;
using GrillBot.Contracts.Unverify.Responses;
using GrillBot.Contracts.Unverify.Responses.Logs;
using GrillBot.Contracts.Unverify.Responses.Logs.Detail;

namespace UnverifyService.Controllers;

public class LogsController(IServiceProvider serviceProvider) : GrillBot.Core.Infrastructure.Actions.ControllerBase(serviceProvider)
{
    [HttpPost("list")]
    [ProducesResponseType<PaginatedResponse<UnverifyLogItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetUnverifyLogsAsync([FromBody] UnverifyLogListRequest request)
        => ProcessAsync<GetUnverifyLogsAction>(request);

    [HttpGet("{id:guid}")]
    [ProducesResponseType<UnverifyLogDetail>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetUnverifyLogDetailAsync(Guid id)
        => ProcessAsync<GetUnverifyLogDetailAction>(id);

    [HttpGet("archive")]
    [ProducesResponseType<ArchivationResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> CreateArchivationDataAsync()
        => ProcessAsync<CreateArchivationDataAction>();
}
