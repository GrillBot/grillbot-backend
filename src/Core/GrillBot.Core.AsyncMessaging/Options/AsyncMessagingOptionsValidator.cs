using GrillBot.Core.AsyncMessaging.Topology;
using Microsoft.Extensions.Options;

namespace GrillBot.Core.AsyncMessaging.Options;

/// <summary>
/// Validates the "AsyncMessaging" section. Every rule lives here rather than in
/// DataAnnotations on <see cref="AsyncMessagingOptions"/>, because the same rules have to
/// run twice: once at startup through <c>ValidateOnStart()</c>, and once while Wolverine is
/// being configured, which happens before the service provider exists. One validator means
/// one wording for both.
/// </summary>
public class AsyncMessagingOptionsValidator : IValidateOptions<AsyncMessagingOptions>
{
    public ValidateOptionsResult Validate(string? name, AsyncMessagingOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.QueueName))
        {
            failures.Add("\"AsyncMessaging:QueueName\" is required - it names the queue this application listens to.");
        }
        else if (!Queues.All.Contains(options.QueueName))
        {
            failures.Add(
                $"\"AsyncMessaging:QueueName\" is \"{options.QueueName}\", which is not a known deployable. See docker/deployables.json."
            );
        }

        if (options.ListenerCount < 1)
            failures.Add($"\"AsyncMessaging:ListenerCount\" is {options.ListenerCount}; at least one listener is required to consume the queue.");

        if (options.MaximumParallelMessages < 1)
            failures.Add($"\"AsyncMessaging:MaximumParallelMessages\" is {options.MaximumParallelMessages}; it must be at least 1.");

        if (options.DefaultExecutionTimeout <= TimeSpan.Zero)
            failures.Add($"\"AsyncMessaging:DefaultExecutionTimeout\" is {options.DefaultExecutionTimeout}; it must be a positive duration.");

        if (options.DeadLetterQueueExpiration <= TimeSpan.Zero)
            failures.Add($"\"AsyncMessaging:DeadLetterQueueExpiration\" is {options.DeadLetterQueueExpiration}; it must be a positive duration.");

        if (options.RetryCooldowns.Length == 0)
            failures.Add("\"AsyncMessaging:RetryCooldowns\" is empty; at least one cooldown is required before a message is moved to the error queue.");
        else if (Array.Exists(options.RetryCooldowns, cooldown => cooldown < TimeSpan.Zero))
            failures.Add("\"AsyncMessaging:RetryCooldowns\" contains a negative duration.");

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
