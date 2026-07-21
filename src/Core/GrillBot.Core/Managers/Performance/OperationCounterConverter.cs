namespace GrillBot.Core.Managers.Performance;

public static class OperationCounterConverter
{
    public static List<OperationStatItem> ComputeTree(List<CounterStats> stats)
    {
        var result = new List<OperationStatItem>();

        foreach (var stat in stats)
            ComputeTree(result, stat);

        return result;
    }

    private static void ComputeTree(List<OperationStatItem> result, CounterStats stats)
    {
        var computedTree = ComputeTreeFromCounter(stats); // Compute new tree from statistics.
        var currentTree = result.Find(o => o.Section == computedTree.Section); // Find root tree to merge.
        if (currentTree == null)
        {
            // New tree. Just add to output-
            result.Add(computedTree);
            return;
        }

        MergeTrees(computedTree, currentTree);
    }

    private static void MergeTrees(OperationStatItem computed, OperationStatItem existing)
    {
        existing.TotalTime += computed.TotalTime;
        existing.Count += computed.Count;

        foreach (var childItem in computed.ChildItems)
        {
            var childInExisting = existing.ChildItems.Find(o => o.Section == childItem.Section);
            if (childInExisting == null)
            {
                existing.ChildItems.Add(childItem);
                continue;
            }

            MergeTrees(childItem, childInExisting);
        }
    }

    private static OperationStatItem ComputeTreeFromCounter(CounterStats stats)
    {
        var fields = stats.Section.Split('.');
        var levels = fields.Select(o => new OperationStatItem { Section = o }).ToList();

        var lastLevel = levels[^1];
        lastLevel.Count = stats.Count;
        lastLevel.TotalTime = stats.TotalTime;

        return ConvertItemsToTree(levels);
    }

    private static OperationStatItem ConvertItemsToTree(List<OperationStatItem> levels)
    {
        var before = levels[0];
        for (var i = 1; i < levels.Count; i++)
        {
            before.ChildItems.Add(levels[i]);
            before = levels[i];
        }

        return levels[0];
    }
}
