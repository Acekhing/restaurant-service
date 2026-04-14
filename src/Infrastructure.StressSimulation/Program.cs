using Infrastructure.StressSimulation.Analysis;
using Infrastructure.StressSimulation.Api;
using Infrastructure.StressSimulation.Configuration;
using Infrastructure.StressSimulation.Dashboard;
using Infrastructure.StressSimulation.Pipeline;
using Infrastructure.StressSimulation.Reporting;
using Spectre.Console;

var stressMode = Environment.GetEnvironmentVariable("STRESS_SIMULATION");

if (stressMode == "web")
{
    RunWebMode(args);
    return;
}

if (stressMode != "true")
{
    AnsiConsole.MarkupLine("[bold red]STRESS_SIMULATION environment variable required.[/]");
    AnsiConsole.MarkupLine("  Console mode: [yellow]STRESS_SIMULATION=true dotnet run[/]");
    AnsiConsole.MarkupLine("  Web mode:     [yellow]STRESS_SIMULATION=web dotnet run[/]");
    return;
}

RunConsoleMode();

// ────────────────────────── Web Mode ──────────────────────────

static void RunWebMode(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddSingleton<SimulationRunnerService>();
    builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

    var app = builder.Build();
    app.UseCors();
    app.MapSimulationEndpoints();
    app.Urls.Add("http://localhost:5180");

    Console.WriteLine("Stress Simulation API running on http://localhost:5180");
    Console.WriteLine("Endpoints:");
    Console.WriteLine("  GET  /api/stress/scenarios");
    Console.WriteLine("  POST /api/stress/run");
    Console.WriteLine("  GET  /api/stress/stream/{runId}");
    Console.WriteLine("  GET  /api/stress/report/{runId}");

    app.Run();
}

// ────────────────────────── Console Mode ──────────────────────────

static void RunConsoleMode()
{
    AnsiConsole.Write(new FigletText("Stress Sim").Color(Color.Cyan1));
    AnsiConsole.MarkupLine("[bold]QC Inventory Platform - Infrastructure Stress Simulation[/]");
    AnsiConsole.MarkupLine("[dim]No real infrastructure is touched. All metrics are simulated.[/]");
    AnsiConsole.WriteLine();

    var config = new SimulationConfig();
    var baseProfile = LoadProfile.Default;
    var scenarios = ScenarioDefinition.AllScenarios;
    var orchestrator = new PipelineOrchestrator(config);
    var dashboard = new ConsoleDashboard();
    var scenarioResults = new Dictionary<string, ScenarioResult>();

    const double tickSeconds = 1.0;

    AnsiConsole.MarkupLine($"[bold]Load Profile:[/] {baseProfile.TotalWritesPerSec:N0} writes/sec, {baseProfile.ReadsPerSec:N0} reads/sec");
    AnsiConsole.MarkupLine($"[bold]Scenarios:[/] {scenarios.Count}");
    AnsiConsole.MarkupLine($"[bold]Simulated scale:[/] {baseProfile.Branches:N0} branches, {baseProfile.InventoryItems:N0} items, {baseProfile.Menus:N0} menus");
    AnsiConsole.WriteLine();

    foreach (var scenario in scenarios)
    {
        orchestrator.Reset();
        var effectiveProfile = scenario.ApplyTo(baseProfile);
        var totalTicks = (int)(scenario.Duration.TotalSeconds / tickSeconds);

        AnsiConsole.Write(new Rule($"[bold yellow]{scenario.Name}[/]"));
        AnsiConsole.MarkupLine($"[dim]{scenario.Description}[/]");
        AnsiConsole.MarkupLine($"[dim]Duration: {scenario.Duration.TotalMinutes:F0} min | Writes: {effectiveProfile.TotalWritesPerSec:N0}/sec | Reads: {effectiveProfile.ReadsPerSec:N0}/sec[/]");

        if (scenario.Failure is not null)
            AnsiConsole.MarkupLine($"[dim red]Failure injection: {scenario.Failure.Component} at {scenario.Failure.ThroughputFactor:P0} throughput ({scenario.Failure.StartsAt.TotalMinutes:F0}-{scenario.Failure.EndsAt.TotalMinutes:F0} min)[/]");

        AnsiConsole.WriteLine();

        long totalWrites = 0;
        long totalErrors = 0;
        double peakBackpressure = 0;
        TickResult? lastTick = null;

        AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn())
            .Start(ctx =>
            {
                var task = ctx.AddTask($"[cyan]{scenario.Name}[/]", maxValue: totalTicks);

                for (var i = 0; i < totalTicks; i++)
                {
                    var elapsed = TimeSpan.FromSeconds(i * tickSeconds);
                    orchestrator.ApplyFailure(scenario.Failure, elapsed);

                    var tick = orchestrator.RunTick(effectiveProfile, tickSeconds, i * tickSeconds);
                    totalWrites += tick.WriteCount;
                    totalErrors += tick.WriteErrors;
                    if (tick.BackpressureLevel > peakBackpressure)
                        peakBackpressure = tick.BackpressureLevel;

                    lastTick = tick;
                    dashboard.Update(scenario.Name, elapsed, tick, tick.BackpressureLevel);

                    if (i % 30 == 0 && i > 0)
                        RenderMiniStatus(tick, elapsed);

                    task.Increment(1);
                }
            });

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

        AnsiConsole.MarkupLine($"  [green]Completed[/] | Writes: {totalWrites:N0} | Errors: {totalErrors:N0} | Peak BP: {peakBackpressure:P0}");

        if (bottlenecks.Count > 0)
        {
            var top = bottlenecks[0];
            AnsiConsole.MarkupLine($"  [yellow]Top bottleneck:[/] {top.Component} (score: {top.Score:F3}, saturation: {top.MaxSaturation:P0})");
        }

        if (alerts.Count > 0)
            AnsiConsole.MarkupLine($"  [red]Alerts:[/] {alerts.Count}");

        AnsiConsole.WriteLine();
    }

    AnsiConsole.Write(new Rule("[bold green]Scaling Forecast[/]"));
    AnsiConsole.MarkupLine("[dim]Running 2x, 5x, 10x load simulations...[/]");

    var forecaster = new ScalingForecaster(config, orchestrator);
    var forecasts = forecaster.Forecast(baseProfile);

    foreach (var f in forecasts)
    {
        var stableText = f.EstimatedStable ? "[green]STABLE[/]" : "[red]UNSTABLE[/]";
        AnsiConsole.MarkupLine($"  {f.Multiplier}x: {stableText} | Top bottleneck: {f.TopBottleneck} ({f.TopBottleneckScore:F2}) | BP: {f.BackpressureLevel:P0}");
    }
    AnsiConsole.WriteLine();

    AnsiConsole.Write(new Rule("[bold green]Infrastructure Recommendations[/]"));
    var baselineSummaries = scenarioResults.TryGetValue("Normal Production Load", out var bl)
        ? bl.Summaries
        : new Dictionary<string, ComponentSummary>();
    var recommendations = InfraRecommender.Recommend(baseProfile, config, baselineSummaries, forecasts);

    var recTable = new Table()
        .Border(TableBorder.Rounded)
        .AddColumn("Component")
        .AddColumn("Setting")
        .AddColumn("Current")
        .AddColumn("[green]Recommended[/]")
        .AddColumn("Reason");

    foreach (var rec in recommendations)
        recTable.AddRow(rec.Component, rec.Setting, rec.CurrentValue, $"[bold green]{rec.RecommendedValue}[/]", Markup.Escape(rec.Reason));

    AnsiConsole.Write(recTable);
    AnsiConsole.WriteLine();

    AnsiConsole.Write(new Rule("[bold]Generating Report[/]"));
    var reportContent = StressReport.Generate(baseProfile, config, scenarioResults, forecasts, recommendations);
    var reportPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "stress-report.md");
    reportPath = Path.GetFullPath(reportPath);
    File.WriteAllText(reportPath, reportContent);
    AnsiConsole.MarkupLine($"[green]Report written to:[/] {reportPath}");

    AnsiConsole.WriteLine();
    AnsiConsole.Write(new Rule("[bold cyan]Simulation Complete[/]"));
    AnsiConsole.MarkupLine("[bold]All scenarios executed successfully. No infrastructure was harmed.[/]");
}

static void RenderMiniStatus(TickResult tick, TimeSpan elapsed)
{
    var bp = tick.BackpressureLevel;
    var bpColor = bp < 0.3 ? "green" : bp < 0.7 ? "yellow" : "red";
    var lag = tick.ConsumerMetrics.Values.Sum(m => m.Lag);

    AnsiConsole.MarkupLine(
        $"    [{bpColor}]t={elapsed:mm\\:ss}[/] | " +
        $"W:{tick.WriteCount:N0} R:{tick.ReadCount:N0} E:{tick.WriteErrors} | " +
        $"BP:[{bpColor}]{bp:P0}[/] | " +
        $"Kafka lag:{lag:N0} | " +
        $"CDC queue:{tick.Snapshots.FirstOrDefault(s => s.Name == "Debezium CDC")?.CurrentQueueDepth:N0}"
    );
}
