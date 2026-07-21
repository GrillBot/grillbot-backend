namespace GrillBot.Core.Services.Common.Executor;

public record ServiceExecutorContext(
    string? AuthorizationToken,
    CancellationToken CancellationToken
);
