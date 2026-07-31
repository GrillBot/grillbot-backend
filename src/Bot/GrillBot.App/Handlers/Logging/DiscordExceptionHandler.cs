using Discord.Interactions;
using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Common.Exceptions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Logging;
using GrillBot.Core.Extensions;
using GrillBot.Contracts.Bot.Events.Errors;
using Wolverine;


namespace GrillBot.App.Handlers.Logging;

public class DiscordExceptionHandler(IMessageBus _rabbitPublisher) : ILoggingHandler
{
    public Task<bool> CanHandleAsync(LogSeverity severity, string source, Exception? exception = null)
    {
        if (exception is null) return Task.FromResult(false);
        if (severity != LogSeverity.Critical && severity != LogSeverity.Error && severity != LogSeverity.Warning) return Task.FromResult(false);

        var ex = exception is InteractionException ? exception.InnerException! : exception;
        return Task.FromResult(!LoggingHelper.IsWarning(source, ex));
    }

    public Task InfoAsync(string source, string message) => Task.CompletedTask;

    public Task WarningAsync(string source, string message, Exception? exception = null)
        => ErrorAsync(source, message, exception!);

    public Task ErrorAsync(string source, string message, Exception exception)
    {
        var notification = CreateErrorNotification(source, message, exception);
        return _rabbitPublisher.PublishAsync(notification).AsTask();
    }

    /// <summary>
    /// The notification is a record, so each Set*Info method collects into a field list and
    /// hands back the parts that are not fields; the payload is built once at the end.
    /// </summary>
    private static ErrorNotificationPayload CreateErrorNotification(string source, string message, Exception exception)
    {
        var fields = new List<ErrorNotificationField>();

        var (title, userId) = exception switch
        {
            ApiException apiException => CollectApiExceptionInfo(fields, apiException, message),
            InteractionException interactionException => CollectInteractionExceptionInfo(fields, interactionException, message),
            JobException jobException => CollectJobExceptionInfo(fields, jobException, source, message),
            FrontendException frontendException => CollectFrontendExceptionInfo(fields, frontendException, source, message),
            _ => CollectCommonExceptionInfo(fields, exception, source, message)
        };

        return new ErrorNotificationPayload(title, fields, userId);
    }

    private static (string Title, ulong? UserId) CollectApiExceptionInfo(List<ErrorNotificationField> fields, ApiException exception, string? message)
    {
        ulong? userId = null;

        if (!string.IsNullOrEmpty(exception.Path))
            fields.Add(new("Adresa", exception.Path, false));
        if (!string.IsNullOrEmpty(exception.ControllerInfo))
            fields.Add(new("Controller", exception.ControllerInfo, false));

        if (exception.LoggedUser is not null)
        {
            userId = exception.LoggedUser.Id;
            fields.Add(new("Přihlášený uživatel", exception.LoggedUser.GetFullName(), false));
        }

        fields.Add(new("Obsah chyby", CreateExceptionContentMessage(message, exception), false));
        return ("Při zpracování požadavku na API došlo k chybě", userId);
    }

    private static (string Title, ulong? UserId) CollectInteractionExceptionInfo(List<ErrorNotificationField> fields, InteractionException exception, string? message)
    {
        var context = exception.InteractionContext;
        var cmd = exception.CommandInfo;

        if (context.Guild is not null)
            fields.Add(new("Server", context.Guild.Name, true));

        fields.Add(new("Kanál", context.Channel.Name, true));
        fields.Add(new("Uživatel", context.User.GetFullName(), false));
        fields.Add(new("Příkaz", $"{cmd.Name} ({cmd.Module}/{cmd.MethodName})", false));
        fields.Add(new("Obsah chyby", CreateExceptionContentMessage(message, exception.InnerException!), false));

        return ("Při provádění příkazu došlo k chybě.", context.User.Id);
    }

    private static (string Title, ulong? UserId) CollectJobExceptionInfo(List<ErrorNotificationField> fields, JobException exception, string source, string? message)
    {
        ulong? userId = null;

        fields.Add(new("Zdroj", source, true));
        fields.Add(new("Typ", exception.InnerException!.GetType().Name, true));

        if (exception.LoggedUser is not null)
        {
            userId = exception.LoggedUser.Id;
            fields.Add(new("Spustil", exception.LoggedUser.GetFullName(), false));
        }

        fields.Add(new("Obsah chyby", CreateExceptionContentMessage(message, exception.InnerException!), false));
        return ("Při běhu naplánované úlohy došlo k chybě.", userId);
    }

    private static (string Title, ulong? UserId) CollectCommonExceptionInfo(List<ErrorNotificationField> fields, Exception exception, string source, string? message)
    {
        fields.Add(new("Zdroj", source, true));
        fields.Add(new("Typ", exception.GetType().Name, true));
        fields.Add(new("Obsah chyby", CreateExceptionContentMessage(message, exception), false));

        return ("Došlo k neočekávané chybě.", null);
    }

    private static (string Title, ulong? UserId) CollectFrontendExceptionInfo(List<ErrorNotificationField> fields, FrontendException exception, string source, string? message)
    {
        fields.Add(new("Zdroj", source, true));
        fields.Add(new("Typ", exception.GetType().Name, true));
        fields.Add(new("Obsah chyby", CreateExceptionContentMessage(message, exception), false));

        return ("Došlo k neočekávané chybě na webu.", exception.LoggedUser.Id);
    }

    private static string CreateExceptionContentMessage(string? message, Exception exception)
    {
        var msg = (!string.IsNullOrEmpty(message) ? message + "\n" : "") + exception.Message;
        return msg.Trim().Cut(EmbedFieldBuilder.MaxFieldValueLength)!;
    }
}
