namespace GrillBot.Core.Infrastructure;

public interface IRequestFilterAction
{
    Task BeforeExecutionAsync(CancellationToken cancellationToken = default);
    Task AfterExecutionAsync(CancellationToken cancellationToken = default);
}
