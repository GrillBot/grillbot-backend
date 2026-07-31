using AuditLogService.Core.Entity;
using GrillBot.Contracts.AuditLog.Events.Recalculation;
using GrillBot.Contracts.AuditLog.Enums;
using AuditLogService.Handlers.Recalculation.Actions;
using AuditLogService.Handlers.Recalculation.Actions.Telemetry;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;

namespace AuditLogService.Handlers.Recalculation;

public class RecalculationHandler(IServiceProvider serviceProvider)
    : EventHandlerBaseWithDb<AuditLogServiceContext>(serviceProvider)
{
    public async Task HandleAsync(RecalculationPayload message, CancellationToken cancellationToken)
    {
        foreach (var action in GetRecalculationActions(message).Where(a => a.CheckPreconditions(message)))
        {
            using (CreateCounter(action.GetType().Name))
                await action.ProcessAsync(message);
        }

        return;
    }

    private IEnumerable<RecalculationActionBase> GetRecalculationActions(RecalculationPayload payload)
    {
        if (payload.Type is LogType.Api or LogType.JobCompleted or LogType.InteractionCommand)
        {
            if (payload.Type is LogType.Api)
            {
                yield return new ApiRequestStatsRecalculationAction(ServiceProvider);
                yield return new ApiUserStatsRecalculationAction(ServiceProvider);
            }

            yield return new DailyAvgTimesRecalculationAction(ServiceProvider);

            if (payload.Type is LogType.InteractionCommand)
            {
                yield return new InteractionStatsRecalculationAction(ServiceProvider);
                yield return new InteractionUserStatsRecalculationAction(ServiceProvider);
            }

            if (payload.Type is LogType.JobCompleted)
                yield return new JobInfoRecalculationAction(ServiceProvider);
        }

        yield return new InvalidStatsRecalculationAction(ServiceProvider);
        yield return new RecalculateLogTelemetryAction(ServiceProvider);

        if (payload.FilesCount > 0)
            yield return new RecalculateFilesTelemetryAction(ServiceProvider);

        if (payload.Type is LogType.Api)
        {
            yield return new RecalculateApiTelemetryAction(ServiceProvider);
        }

        if (payload.Type is LogType.JobCompleted)
        {
            yield return new RecalculateJobsTelemetryAction(ServiceProvider);
        }
    }
}
