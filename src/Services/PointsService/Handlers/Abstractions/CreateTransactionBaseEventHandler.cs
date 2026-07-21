using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Contracts.AuditLog.Events.Create;
using PointsService.Core.Entity;
using GrillBot.Contracts.Points.Events;

namespace PointsService.Handlers.Abstractions;

public abstract class CreateTransactionBaseEventHandler<TPayload>(
    IServiceProvider serviceProvider
) : BasePointsEvent<TPayload>(serviceProvider) where TPayload : CreateTransactionBasePayload, new()
{
    protected async Task<bool> ValidationFailedAsync(TPayload payload, string? channelId, string message, bool suppressAudit = false)
    {
        if (Logger.IsEnabled(LogLevel.Warning))
            Logger.LogWarning("{Message}", message);

        if (!suppressAudit)
            await WriteValidationErrorToLogAsync(payload, channelId, message);

        return false;
    }

    private Task WriteValidationErrorToLogAsync(TPayload payload, string? channelId, string message)
    {
        var logRequest = new LogRequest(LogType.Warning, DateTime.UtcNow, payload.GuildId, null, channelId)
        {
            LogMessage = new LogMessageRequest
            {
                Message = message,
                Source = GetType().Name,
                SourceAppName = "PointsService"
            }
        };

        return Publisher.PublishAsync(new CreateItemsMessage(logRequest));
    }

    protected async Task CommitTransactionAsync(Transaction transaction)
    {
        await DbContext.AddAsync(transaction);
        await ContextHelper.SaveChangesAsync();
    }
}
