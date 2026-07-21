using System.Diagnostics;
using GrillBot.Core.Database;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Services.Diagnostics.Models;

namespace GrillBot.Core.Services.Diagnostics;

public class DiagnosticsProvider(DiagnosticsManager _manager, ICounterManager _counterManager) : IDiagnosticsProvider
{
    private readonly IStatisticsProvider? _statisticsProvider;

    public DiagnosticsProvider(DiagnosticsManager manager, IStatisticsProvider statisticsProvider, ICounterManager counterManager) : this(manager, counterManager)
    {
        _statisticsProvider = statisticsProvider;
    }

    public async Task<DiagnosticInfo> GetInfoAsync(CancellationToken cancellationToken = default)
    {
        var process = Process.GetCurrentProcess();
        var databaseStatistics = _statisticsProvider is null ? null : await _statisticsProvider.GetTableStatisticsAsync(cancellationToken);
        var operationStats = _counterManager.GetStatistics();
        var now = DateTime.Now;

        return new DiagnosticInfo
        {
            Endpoints = _manager.Statistics,
            Uptime = Convert.ToInt64((now - process.StartTime).TotalMilliseconds),
            CpuTime = Convert.ToInt64(process.TotalProcessorTime.TotalMilliseconds),
            MeasuredFrom = process.StartTime,
            RequestsCount = _manager.Statistics.Sum(o => o.Count),
            UsedMemory = process.WorkingSet64,
            DatabaseStatistics = databaseStatistics,
            Operations = OperationCounterConverter.ComputeTree(operationStats)
        };
    }
}
