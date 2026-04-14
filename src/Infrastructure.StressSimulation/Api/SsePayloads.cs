using Infrastructure.StressSimulation.Analysis;
using Infrastructure.StressSimulation.Reporting;
using Infrastructure.StressSimulation.Simulators;

namespace Infrastructure.StressSimulation.Api;

public sealed class SseEvent
{
    public required string Type { get; init; }
    public required object Data { get; init; }
}

public sealed class ScenarioInfo
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required double DurationMinutes { get; init; }
    public double WriteMultiplier { get; init; } = 1.0;
    public double ReadMultiplier { get; init; } = 1.0;
    public string? FailureComponent { get; init; }
}

public sealed class RunRequest
{
    public string[]? ScenarioNames { get; init; }
    public long? Branches { get; init; }
    public long? InventoryItems { get; init; }
    public long? Menus { get; init; }
    public int? ConcurrentUsers { get; init; }
}

public sealed class RunResponse
{
    public required string RunId { get; init; }
}

public sealed class TickPayload
{
    public required string Scenario { get; init; }
    public double ElapsedSeconds { get; init; }
    public double DurationSeconds { get; init; }
    public int WriteCount { get; init; }
    public long ReadCount { get; init; }
    public int WriteErrors { get; init; }
    public double AvgWriteLatencyMs { get; init; }
    public double AvgReadLatencyMs { get; init; }
    public double BackpressureLevel { get; init; }
    public double OutboxEventsGenerated { get; init; }
    public double DebeziumProcessed { get; init; }
    public List<SnapshotPayload> Snapshots { get; init; } = [];
    public Dictionary<string, ConsumerLagPayload> ConsumerMetrics { get; init; } = new();
}

public sealed class SnapshotPayload
{
    public required string Name { get; init; }
    public double P50LatencyMs { get; init; }
    public double P95LatencyMs { get; init; }
    public double P99LatencyMs { get; init; }
    public double SaturationLevel { get; init; }
    public double CurrentQueueDepth { get; init; }
    public long TotalProcessed { get; init; }
    public long TotalErrors { get; init; }
}

public sealed class ConsumerLagPayload
{
    public long Lag { get; init; }
    public long Consumed { get; init; }
    public double LatencyMs { get; init; }
}

public sealed class ScenarioStartPayload
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public double DurationSeconds { get; init; }
    public double WritesPerSec { get; init; }
    public double ReadsPerSec { get; init; }
}

public sealed class ScenarioEndPayload
{
    public required string Name { get; init; }
    public long TotalWrites { get; init; }
    public long TotalErrors { get; init; }
    public double PeakBackpressure { get; init; }
    public List<BottleneckPayload> Bottlenecks { get; init; } = [];
    public int AlertCount { get; init; }
}

public sealed class BottleneckPayload
{
    public required string Component { get; init; }
    public double Score { get; init; }
    public double MaxSaturation { get; init; }
    public double MaxP99LatencyMs { get; init; }
    public long TotalErrors { get; init; }
}

public sealed class ForecastPayload
{
    public double Multiplier { get; init; }
    public double TotalWritesPerSec { get; init; }
    public double TotalReadsPerSec { get; init; }
    public bool EstimatedStable { get; init; }
    public required string TopBottleneck { get; init; }
    public double TopBottleneckScore { get; init; }
    public double BackpressureLevel { get; init; }
}

public sealed class RecommendationPayload
{
    public required string Component { get; init; }
    public required string Setting { get; init; }
    public required string CurrentValue { get; init; }
    public required string RecommendedValue { get; init; }
    public required string Reason { get; init; }
}

public static class PayloadMapper
{
    public static SnapshotPayload ToPayload(SimulatorSnapshot s) => new()
    {
        Name = s.Name,
        P50LatencyMs = Math.Round(s.P50LatencyMs, 2),
        P95LatencyMs = Math.Round(s.P95LatencyMs, 2),
        P99LatencyMs = Math.Round(s.P99LatencyMs, 2),
        SaturationLevel = Math.Round(s.SaturationLevel, 4),
        CurrentQueueDepth = Math.Round(s.CurrentQueueDepth, 0),
        TotalProcessed = s.TotalProcessed,
        TotalErrors = s.TotalErrors
    };

    public static BottleneckPayload ToPayload(BottleneckEntry b) => new()
    {
        Component = b.Component,
        Score = Math.Round(b.Score, 4),
        MaxSaturation = Math.Round(b.MaxSaturation, 4),
        MaxP99LatencyMs = Math.Round(b.MaxP99LatencyMs, 2),
        TotalErrors = b.TotalErrors
    };

    public static ForecastPayload ToPayload(ScalingForecast f) => new()
    {
        Multiplier = f.Multiplier,
        TotalWritesPerSec = Math.Round(f.TotalWritesPerSec, 0),
        TotalReadsPerSec = Math.Round(f.TotalReadsPerSec, 0),
        EstimatedStable = f.EstimatedStable,
        TopBottleneck = f.TopBottleneck,
        TopBottleneckScore = Math.Round(f.TopBottleneckScore, 4),
        BackpressureLevel = Math.Round(f.BackpressureLevel, 4)
    };

    public static RecommendationPayload ToPayload(InfraRecommendation r) => new()
    {
        Component = r.Component,
        Setting = r.Setting,
        CurrentValue = r.CurrentValue,
        RecommendedValue = r.RecommendedValue,
        Reason = r.Reason
    };
}
