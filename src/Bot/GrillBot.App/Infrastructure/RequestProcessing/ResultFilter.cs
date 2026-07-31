using GrillBot.Common.Models;
using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Contracts.AuditLog.Events.Create;
using Microsoft.AspNetCore.Mvc.Filters;
using Wolverine;


namespace GrillBot.App.Infrastructure.RequestProcessing;

public class ResultFilter : IAsyncResultFilter
{
    private ApiRequestContext ApiRequestContext { get; }

    private readonly IMessageBus _rabbitPublisher;

    public ResultFilter(ApiRequestContext apiRequestContext, IMessageBus rabbitPublisher)
    {
        ApiRequestContext = apiRequestContext;
        _rabbitPublisher = rabbitPublisher;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next();

        var response = context.HttpContext.Response;
        ApiRequestContext.LogRequest.Result = $"{response.StatusCode} ({(HttpStatusCode)response.StatusCode})";
        ApiRequestContext.LogRequest.EndAt = DateTime.UtcNow;

        await WriteToAuditLogAsync();
    }

    private Task WriteToAuditLogAsync()
    {
        var userId = ApiRequestContext.GetUserId().ToString();
        if (userId == "0") userId = null;

        var logRequest = new LogRequest(LogType.Api, DateTime.UtcNow, null, userId)
        {
            ApiRequest = ApiRequestContext.LogRequest,
        };

        return _rabbitPublisher.PublishAsync(new CreateItemsMessage(logRequest)).AsTask();
    }
}
