using System.Text;
using Infrastructure.StressSimulation.Analysis;
using Infrastructure.StressSimulation.Configuration;

namespace Infrastructure.StressSimulation.Reporting;

public static class StressReport
{
    public static string Generate(
        LoadProfile profile,
        SimulationConfig config,
        Dictionary<string, ScenarioResult> scenarioResults,
        List<ScalingForecast> forecasts,
        List<InfraRecommendation> recommendations)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Infrastructure Stress Simulation Report");
        sb.AppendLine();
        sb.AppendLine($"**Generated:** {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        WriteLoadProfile(sb, profile);
        WriteSystemLimits(sb, scenarioResults);
        WriteScenarioResults(sb, scenarioResults);
        WriteScalingForecast(sb, forecasts);
        WriteRecommendations(sb, recommendations);
        WriteBottleneckRanking(sb, scenarioResults);

        return sb.ToString();
    }

    private static void WriteLoadProfile(StringBuilder sb, LoadProfile profile)
    {
        sb.AppendLine("## Load Profile");
        sb.AppendLine();
        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|--------|-------|");
        sb.AppendLine($"| Branches | {profile.Branches:N0} |");
        sb.AppendLine($"| Inventory Items | {profile.InventoryItems:N0} |");
        sb.AppendLine($"| Menus | {profile.Menus:N0} |");
        sb.AppendLine($"| Concurrent Users | {profile.ConcurrentUsers:N0} |");
        sb.AppendLine($"| Availability Toggles/min | {profile.AvailabilityTogglesPerMin:N0} |");
        sb.AppendLine($"| Price Updates/min | {profile.PriceUpdatesPerMin:N0} |");
        sb.AppendLine($"| Branch Updates/min | {profile.BranchUpdatesPerMin:N0} |");
        sb.AppendLine($"| Reads/sec | {profile.ReadsPerSec:N0} |");
        sb.AppendLine($"| **Total Writes/sec** | **{profile.TotalWritesPerSec:N0}** |");
        sb.AppendLine($"| **Total Outbox Events/sec** | **{profile.TotalOutboxEventsPerSec:N0}** |");
        sb.AppendLine();
    }

    private static void WriteSystemLimits(StringBuilder sb, Dictionary<string, ScenarioResult> results)
    {
        sb.AppendLine("## System Limits");
        sb.AppendLine();

        if (results.TryGetValue("Normal Production Load", out var baseline))
        {
            var maxEventsPerSec = baseline.Summaries.Values
                .Where(s => s.TotalProcessed > 0)
                .Min(s => s.TotalProcessed / Math.Max(1, baseline.DurationSeconds));

            sb.AppendLine($"- **Max Sustainable Events/sec:** ~{maxEventsPerSec:N0}");
            sb.AppendLine($"- **Peak Backpressure:** {baseline.PeakBackpressure:P0}");

            var topBottleneck = baseline.Bottlenecks.FirstOrDefault();
            if (topBottleneck is not null)
                sb.AppendLine($"- **Primary Bottleneck:** {topBottleneck.Component} (score: {topBottleneck.Score:F2})");
        }

        sb.AppendLine();
        sb.AppendLine("### Estimated Breaking Points");
        sb.AppendLine();
        foreach (var (name, result) in results.Where(r => r.Value.PeakBackpressure > 0.7))
        {
            sb.AppendLine($"- **{name}:** backpressure reached {result.PeakBackpressure:P0}, " +
                         $"top bottleneck: {result.Bottlenecks.FirstOrDefault()?.Component ?? "N/A"}");
        }
        sb.AppendLine();
    }

    private static void WriteScenarioResults(StringBuilder sb, Dictionary<string, ScenarioResult> results)
    {
        sb.AppendLine("## Scenario Results");
        sb.AppendLine();
        sb.AppendLine("| Scenario | Duration | Writes | Errors | Peak Backpressure | Top Bottleneck |");
        sb.AppendLine("|----------|----------|--------|--------|-------------------|----------------|");

        foreach (var (name, result) in results)
        {
            var topBottleneck = result.Bottlenecks.FirstOrDefault()?.Component ?? "None";
            sb.AppendLine($"| {name} | {result.DurationSeconds:F0}s | {result.TotalWrites:N0} | {result.TotalErrors:N0} | {result.PeakBackpressure:P0} | {topBottleneck} |");
        }
        sb.AppendLine();

        foreach (var (name, result) in results)
        {
            sb.AppendLine($"### {name}");
            sb.AppendLine();
            sb.AppendLine("| Component | Avg P99 Latency | Max P99 Latency | Max Saturation | Max Queue | Errors |");
            sb.AppendLine("|-----------|----------------|-----------------|----------------|-----------|--------|");

            foreach (var (comp, summary) in result.Summaries.OrderByDescending(s => s.Value.MaxSaturation))
            {
                sb.AppendLine($"| {comp} | {summary.AvgP99LatencyMs:F1}ms | {summary.MaxP99LatencyMs:F1}ms | {summary.MaxSaturation:P0} | {summary.MaxQueueDepth:N0} | {summary.TotalErrors:N0} |");
            }
            sb.AppendLine();

            if (result.Alerts.Count > 0)
            {
                sb.AppendLine($"**Alerts ({result.Alerts.Count} total):**");
                foreach (var alert in result.Alerts.Take(10))
                    sb.AppendLine($"- [{alert.Type}] {alert.Message}");
                if (result.Alerts.Count > 10)
                    sb.AppendLine($"- ... and {result.Alerts.Count - 10} more");
                sb.AppendLine();
            }
        }
    }

    private static void WriteScalingForecast(StringBuilder sb, List<ScalingForecast> forecasts)
    {
        sb.AppendLine("## Scaling Forecast");
        sb.AppendLine();
        sb.AppendLine("| Multiplier | Writes/sec | Reads/sec | Stable? | Top Bottleneck | Score | Backpressure |");
        sb.AppendLine("|------------|-----------|-----------|---------|----------------|-------|-------------|");

        foreach (var f in forecasts)
        {
            sb.AppendLine($"| {f.Multiplier}x | {f.TotalWritesPerSec:N0} | {f.TotalReadsPerSec:N0} | {(f.EstimatedStable ? "Yes" : "**NO**")} | {f.TopBottleneck} | {f.TopBottleneckScore:F2} | {f.BackpressureLevel:P0} |");
        }
        sb.AppendLine();
    }

    private static void WriteRecommendations(StringBuilder sb, List<InfraRecommendation> recommendations)
    {
        sb.AppendLine("## Infrastructure Recommendations");
        sb.AppendLine();
        sb.AppendLine("| Component | Setting | Current | Recommended | Reason |");
        sb.AppendLine("|-----------|---------|---------|-------------|--------|");

        foreach (var rec in recommendations)
        {
            sb.AppendLine($"| {rec.Component} | {rec.Setting} | {rec.CurrentValue} | **{rec.RecommendedValue}** | {rec.Reason} |");
        }
        sb.AppendLine();
    }

    private static void WriteBottleneckRanking(StringBuilder sb, Dictionary<string, ScenarioResult> results)
    {
        sb.AppendLine("## Bottleneck Ranking (Normal Load)");
        sb.AppendLine();

        if (results.TryGetValue("Normal Production Load", out var baseline))
        {
            sb.AppendLine("| Rank | Component | Score | Max Saturation | Max P99 Latency | Errors |");
            sb.AppendLine("|------|-----------|-------|----------------|-----------------|--------|");

            var rank = 1;
            foreach (var b in baseline.Bottlenecks)
            {
                sb.AppendLine($"| {rank++} | {b.Component} | {b.Score:F3} | {b.MaxSaturation:P0} | {b.MaxP99LatencyMs:F1}ms | {b.TotalErrors:N0} |");
            }
        }
        sb.AppendLine();
    }
}

public sealed class ScenarioResult
{
    public required string Name { get; init; }
    public double DurationSeconds { get; init; }
    public long TotalWrites { get; init; }
    public long TotalErrors { get; init; }
    public double PeakBackpressure { get; init; }
    public Dictionary<string, ComponentSummary> Summaries { get; init; } = new();
    public List<BottleneckEntry> Bottlenecks { get; init; } = [];
    public List<BackpressureAlert> Alerts { get; init; } = [];
}
