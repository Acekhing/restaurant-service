using System.Collections.Concurrent;
using System.Threading.Channels;
using Infrastructure.StressSimulation.Analysis;
using Infrastructure.StressSimulation.Configuration;
using Infrastructure.StressSimulation.Pipeline;
using Infrastructure.StressSimulation.Reporting;

namespace Infrastructure.StressSimulation.Api;

public sealed class SimulationRunnerService
{
    private readonly ConcurrentDictionary<string, SimulationRun> _runs = new();

    public SimulationRun StartRun(
        string[] scenarioNames,
        long? branches = null,
        long? inventoryItems = null,
        long? menus = null,
        int? concurrentUsers = null)
    {
        var runId = Guid.NewGuid().ToString("N")[..12];
        var channel = Channel.CreateUnbounded<SseEvent>(new UnboundedChannelOptions
        {
            SingleWriter = true,
            SingleReader = false
        });

        var run = new SimulationRun
        {
            RunId = runId,
            Channel = channel,
            ScenarioNames = scenarioNames,
            Branches = branches,
            InventoryItems = inventoryItems,
            Menus = menus,
            ConcurrentUsers = concurrentUsers,
            StartedAt = DateTimeOffset.UtcNow
        };

        _runs[runId] = run;

        _ = Task.Run(() => ExecuteRunAsync(run));
        return run;
    }

    public SimulationRun? GetRun(string runId) =>
        _runs.TryGetValue(runId, out var run) ? run : null;

    private async Task ExecuteRunAsync(SimulationRun run)
    {
        var writer = run.Channel.Writer;
        try
        {
            var defaults = LoadProfile.Default;
            var baseProfile = LoadProfile.FromEntityCounts(
                run.Branches ?? defaults.Branches,
                run.InventoryItems ?? defaults.InventoryItems,
                run.Menus ?? defaults.Menus,
                run.ConcurrentUsers ?? defaults.ConcurrentUsers);

            var config = new SimulationConfig
            {
                Elasticsearch = new ElasticsearchConfig
                {
                    InventoryDocCount = baseProfile.InventoryItems,
                    MenuDocCount = baseProfile.Menus,
                    BranchDocCount = baseProfile.Branches,
                }
            };
            var allScenarios = ScenarioDefinition.AllScenarios;
            var orchestrator = new PipelineOrchestrator(config);
            var scenarioResults = new Dictionary<string, ScenarioResult>();

            var selected = run.ScenarioNames.Length > 0
                ? allScenarios.Where(s => run.ScenarioNames.Contains(s.Name)).ToList()
                : allScenarios.ToList();

            if (selected.Count == 0)
                selected = allScenarios.ToList();

            const double tickSeconds = 1.0;

            foreach (var scenario in selected)
            {
                orchestrator.Reset();
                var effectiveProfile = scenario.ApplyTo(baseProfile);
                var totalTicks = (int)(scenario.Duration.TotalSeconds / tickSeconds);

                await writer.WriteAsync(new SseEvent
                {
                    Type = "scenario-start",
                    Data = new ScenarioStartPayload
                    {
                        Name = scenario.Name,
                        Description = scenario.Description,
                        DurationSeconds = scenario.Duration.TotalSeconds,
                        WritesPerSec = effectiveProfile.TotalWritesPerSec,
                        ReadsPerSec = effectiveProfile.ReadsPerSec
                    }
                });

                long totalWrites = 0;
                long totalErrors = 0;
                double peakBackpressure = 0;

                for (var i = 0; i < totalTicks; i++)
                {
                    var elapsed = TimeSpan.FromSeconds(i * tickSeconds);
                    orchestrator.ApplyFailure(scenario.Failure, elapsed);

                    var tick = orchestrator.RunTick(effectiveProfile, tickSeconds, i * tickSeconds);
                    totalWrites += tick.WriteCount;
                    totalErrors += tick.WriteErrors;
                    if (tick.BackpressureLevel > peakBackpressure)
                        peakBackpressure = tick.BackpressureLevel;

                    await writer.WriteAsync(new SseEvent
                    {
                        Type = "tick",
                        Data = new TickPayload
                        {
                            Scenario = scenario.Name,
                            ElapsedSeconds = i * tickSeconds,
                            DurationSeconds = scenario.Duration.TotalSeconds,
                            WriteCount = tick.WriteCount,
                            ReadCount = tick.ReadCount,
                            WriteErrors = tick.WriteErrors,
                            AvgWriteLatencyMs = Math.Round(tick.AvgWriteLatencyMs, 2),
                            AvgReadLatencyMs = Math.Round(tick.AvgReadLatencyMs, 2),
                            BackpressureLevel = Math.Round(tick.BackpressureLevel, 4),
                            OutboxEventsGenerated = Math.Round(tick.OutboxEventsGenerated, 0),
                            DebeziumProcessed = Math.Round(tick.DebeziumProcessed, 0),
                            Snapshots = tick.Snapshots.Select(PayloadMapper.ToPayload).ToList(),
                            ConsumerMetrics = tick.ConsumerMetrics.ToDictionary(
                                kv => kv.Key,
                                kv => new ConsumerLagPayload
                                {
                                    Lag = kv.Value.Lag,
                                    Consumed = kv.Value.Consumed,
                                    LatencyMs = Math.Round(kv.Value.LatencyMs, 2)
                                })
                        }
                    });

                    if (i % 5 == 0)
                        await Task.Yield();
                }

                var summaries = orchestrator.Metrics.GetSummaries();
                var bottlenecks = BottleneckRanker.Rank(summaries);
                var alerts = orchestrator.Backpressure.Alerts.ToList();

                scenarioResults[scenario.Name] = new ScenarioResult
                {
                    Name = scenario.Name,
                    DurationSeconds = scenario.Duration.TotalSeconds,
                    TotalWrites = totalWrites,
                    TotalErrors = totalErrors,
                    PeakBackpressure = peakBackpressure,
                    Summaries = summaries,
                    Bottlenecks = bottlenecks,
                    Alerts = alerts
                };

                await writer.WriteAsync(new SseEvent
                {
                    Type = "scenario-end",
                    Data = new ScenarioEndPayload
                    {
                        Name = scenario.Name,
                        TotalWrites = totalWrites,
                        TotalErrors = totalErrors,
                        PeakBackpressure = Math.Round(peakBackpressure, 4),
                        Bottlenecks = bottlenecks.Select(PayloadMapper.ToPayload).ToList(),
                        AlertCount = alerts.Count
                    }
                });
            }

            var forecaster = new ScalingForecaster(config, orchestrator);
            var forecasts = forecaster.Forecast(baseProfile);

            await writer.WriteAsync(new SseEvent
            {
                Type = "forecast",
                Data = forecasts.Select(PayloadMapper.ToPayload).ToList()
            });

            var baselineSummaries = scenarioResults.TryGetValue("Normal Production Load", out var bl)
                ? bl.Summaries
                : new Dictionary<string, ComponentSummary>();
            var recommendations = InfraRecommender.Recommend(baseProfile, config, baselineSummaries, forecasts);

            await writer.WriteAsync(new SseEvent
            {
                Type = "recommendations",
                Data = recommendations.Select(PayloadMapper.ToPayload).ToList()
            });

            var reportContent = StressReport.Generate(baseProfile, config, scenarioResults, forecasts, recommendations);
            run.Report = reportContent;

            await writer.WriteAsync(new SseEvent
            {
                Type = "complete",
                Data = new { message = "Simulation complete", reportAvailable = true }
            });
        }
        catch (Exception ex)
        {
            await writer.WriteAsync(new SseEvent
            {
                Type = "error",
                Data = new { message = ex.Message }
            });
        }
        finally
        {
            run.IsComplete = true;
            writer.Complete();
        }
    }
}

public sealed class SimulationRun
{
    public required string RunId { get; init; }
    public required Channel<SseEvent> Channel { get; init; }
    public required string[] ScenarioNames { get; init; }
    public long? Branches { get; init; }
    public long? InventoryItems { get; init; }
    public long? Menus { get; init; }
    public int? ConcurrentUsers { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public bool IsComplete { get; set; }
    public string? Report { get; set; }
}
