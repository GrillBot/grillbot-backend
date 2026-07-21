namespace GrillBot.Core.Database;

public interface IStatisticsProvider
{
    Task<Dictionary<string, long>> GetTableStatisticsAsync(CancellationToken cancellationToken = default);
}
