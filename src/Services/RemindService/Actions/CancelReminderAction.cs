using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Contracts.AuditLog.Events.Create;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Managers.Performance;
using GrillBot.Services.Common.Infrastructure.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RemindService.Core.Entity;
using GrillBot.Contracts.Remind.Events;
using GrillBot.Contracts.Remind.Requests;
using RemindService.Options;
using Wolverine;


namespace RemindService.Actions;

public class CancelReminderAction(
    ICounterManager counterManager,
    RemindServiceContext dbContext,
    IMessageBus _publisher
) : ApiAction<RemindServiceContext>(counterManager, dbContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var request = GetParameter<CancelReminderRequest>(0);

        var query = DbContext.RemindMessages.Where(o => o.Id == request.RemindId);
        var remind = await ContextHelper.ReadFirstOrDefaultEntityAsync(query);
        var modelState = CheckRemindStatus(remind, request);
        if (!modelState.IsValid)
            return ApiResult.BadRequest(new ValidationProblemDetails(modelState));

        await WriteToAuditLogAsync(remind!, request);
        await NotifyUserIfPossibleAsync(remind!, request);
        await ContextHelper.SaveChangesAsync();

        return ApiResult.Ok();
    }

    private static ModelStateDictionary CheckRemindStatus(RemindMessage? message, CancelReminderRequest request)
    {
        var modelState = new ModelStateDictionary();

        if (message is null)
        {
            modelState.AddModelError(nameof(request.RemindId), "RemindModule/CancelRemind/NotFound");
            return modelState;
        }

        if (!string.IsNullOrEmpty(message.NotificationMessageId))
            modelState.AddModelError("Remind", "RemindModule/CancelRemind/AlreadyNotified");

        if (message.NotificationMessageId == AppOptions.FinishedUnsentMessageId)
            modelState.AddModelError("Remind", "RemindModule/CancelRemind/AlreadyCancelled");

        if (message.IsSendInProgress)
            modelState.AddModelError("Remind", "RemindModule/RemindInProgress");

        if (message.FromUserId != request.ExecutingUserId && message.ToUserId != request.ExecutingUserId && !request.IsAdminExecution)
            modelState.AddModelError("Remind", "RemindModule/CancelRemind/InvalidOperator");

        return modelState;
    }

    private Task NotifyUserIfPossibleAsync(RemindMessage message, CancelReminderRequest request)
    {
        if (!request.NotifyUser)
        {
            message.IsSendInProgress = false;
            message.NotificationMessageId = AppOptions.FinishedUnsentMessageId;
            return Task.CompletedTask;
        }

        var payload = new SendRemindNotificationPayload(request.RemindId, true);
        return _publisher.PublishAsync(payload).AsTask();
    }

    private Task WriteToAuditLogAsync(RemindMessage message, CancelReminderRequest request)
    {
        var messageTemplate = request.NotifyUser ?
            "The reminder with ID {0} has been canceled. A notification was sent to the user upon cancellation." :
            "The reminder with ID {0} has been canceled.";

        var logRequest = new LogRequest(LogType.Info, DateTime.UtcNow, null, request.ExecutingUserId)
        {
            LogMessage = new LogMessageRequest
            {
                Message = string.Format(messageTemplate, message.Id),
                Source = nameof(CancelReminderAction),
                SourceAppName = "RemindService"
            }
        };

        return _publisher.PublishAsync(new CreateItemsMessage(logRequest)).AsTask();
    }
}
