namespace Infrastructure.StressSimulation.Analysis;

public static class BottleneckRanker
{
    public static List<BottleneckEntry> Rank(Dictionary<string, ComponentSummary> summaries)
    {
        var entries = summaries.Values
            .Select(s => new BottleneckEntry
            {
                Component = s.Name,
                Score = ComputeScore(s),
                MaxSaturation = s.MaxSaturation,
                MaxP99LatencyMs = s.MaxP99LatencyMs,
                MaxQueueDepth = s.MaxQueueDepth,
                TotalErrors = s.TotalErrors
            })
            .OrderByDescending(e => e.Score)
            .ToList();

        return entries;
    }

    private static double ComputeScore(ComponentSummary s)
    {
        var saturationWeight = 0.4;
        var latencyWeight = 0.3;
        var queueWeight = 0.2;
        var errorWeight = 0.1;

        var normalizedLatency = Math.Min(1.0, s.MaxP99LatencyMs / 2000.0);
        var normalizedQueue = Math.Min(1.0, s.MaxQueueDepth / 100_000.0);
        var normalizedErrors = s.TotalProcessed > 0
            ? Math.Min(1.0, (double)s.TotalErrors / s.TotalProcessed * 10)
            : 0;

        return s.MaxSaturation * saturationWeight
            + normalizedLatency * latencyWeight
            + normalizedQueue * queueWeight
            + normalizedErrors * errorWeight;
    }
}

public sealed class BottleneckEntry
{
    public required string Component { get; init; }
    public double Score { get; init; }
    public double MaxSaturation { get; init; }
    public double MaxP99LatencyMs { get; init; }
    public double MaxQueueDepth { get; init; }
    public long TotalErrors { get; init; }
}
