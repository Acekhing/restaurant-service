using Infrastructure.StressSimulation.Analysis;
using Infrastructure.StressSimulation.Configuration;
using Infrastructure.StressSimulation.Pipeline;

namespace Infrastructure.StressSimulation.Reporting;

public sealed class ScalingForecaster
{
    private readonly SimulationConfig _config;
    private readonly PipelineOrchestrator _orchestrator;

    public ScalingForecaster(SimulationConfig config, PipelineOrchestrator orchestrator)
    {
        _config = config;
        _orchestrator = orchestrator;
    }

    public List<ScalingForecast> Forecast(LoadProfile baseline)
    {
        var multipliers = new[] { 2.0, 5.0, 10.0 };
        var forecasts = new List<ScalingForecast>();

        foreach (var mult in multipliers)
        {
            var scaled = baseline.Scale(mult);
            _orchestrator.Reset();

            var tickSeconds = 1.0;
            var totalSeconds = 60.0;
            var ticks = (int)(totalSeconds / tickSeconds);
            TickResult? lastResult = null;

            for (var i = 0; i < ticks; i++)
            {
                lastResult = _orchestrator.RunTick(scaled, tickSeconds, i * tickSeconds);
            }

            var summaries = _orchestrator.Metrics.GetSummaries();
            var bottlenecks = BottleneckRanker.Rank(summaries);

            forecasts.Add(new ScalingForecast
            {
                Multiplier = mult,
                TotalWritesPerSec = scaled.TotalWritesPerSec,
                TotalReadsPerSec = scaled.ReadsPerSec,
                TopBottleneck = bottlenecks.FirstOrDefault()?.Component ?? "None",
                TopBottleneckScore = bottlenecks.FirstOrDefault()?.Score ?? 0,
                BackpressureLevel = lastResult?.BackpressureLevel ?? 0,
                Summaries = summaries,
                EstimatedStable = bottlenecks.All(b => b.Score < 0.7)
            });
        }

        _orchestrator.Reset();
        return forecasts;
    }
}

public sealed class ScalingForecast
{
    public double Multiplier { get; init; }
    public double TotalWritesPerSec { get; init; }
    public double TotalReadsPerSec { get; init; }
    public required string TopBottleneck { get; init; }
    public double TopBottleneckScore { get; init; }
    public double BackpressureLevel { get; init; }
    public Dictionary<string, ComponentSummary> Summaries { get; init; } = new();
    public bool EstimatedStable { get; init; }
}
