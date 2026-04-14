using Infrastructure.StressSimulation.Analysis;
using Infrastructure.StressSimulation.Pipeline;
using Infrastructure.StressSimulation.Simulators;
using Spectre.Console;

namespace Infrastructure.StressSimulation.Dashboard;

public sealed class ConsoleDashboard
{
    private readonly object _lock = new();
    private string _scenarioName = "";
    private TimeSpan _elapsed;
    private TickResult? _lastTick;
    private double _backpressure;

    public void Update(string scenarioName, TimeSpan elapsed, TickResult tick, double backpressure)
    {
        lock (_lock)
        {
            _scenarioName = scenarioName;
            _elapsed = elapsed;
            _lastTick = tick;
            _backpressure = backpressure;
        }
    }

    public void Render()
    {
        TickResult? tick;
        string scenario;
        TimeSpan elapsed;
        double bp;

        lock (_lock)
        {
            tick = _lastTick;
            scenario = _scenarioName;
            elapsed = _elapsed;
            bp = _backpressure;
        }

        if (tick is null) return;

        Console.Clear();
        var header = new Rule($"[bold cyan]STRESS SIMULATION[/] - [yellow]{Markup.Escape(scenario)}[/]");
        AnsiConsole.Write(header);
        AnsiConsole.WriteLine();

        var overviewTable = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Metric")
            .AddColumn("Value");

        overviewTable.AddRow("Elapsed", $"{elapsed:mm\\:ss}");
        overviewTable.AddRow("Writes/tick", $"{tick.WriteCount:N0}");
        overviewTable.AddRow("Reads/tick", $"{tick.ReadCount:N0} (sampled)");
        overviewTable.AddRow("Write Errors", $"{tick.WriteErrors:N0}");
        overviewTable.AddRow("Avg Write Latency", $"{tick.AvgWriteLatencyMs:F2}ms");
        overviewTable.AddRow("Avg Read Latency", $"{tick.AvgReadLatencyMs:F2}ms");

        var bpColor = bp < 0.3 ? "green" : bp < 0.7 ? "yellow" : "red";
        overviewTable.AddRow("Backpressure", $"[{bpColor}]{bp:P0}[/]");

        AnsiConsole.Write(overviewTable);
        AnsiConsole.WriteLine();

        var metricsTable = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Component Metrics[/]")
            .AddColumn("Component")
            .AddColumn("P50 (ms)")
            .AddColumn("P95 (ms)")
            .AddColumn("P99 (ms)")
            .AddColumn("Queue Depth")
            .AddColumn("Saturation")
            .AddColumn("Processed")
            .AddColumn("Errors");

        foreach (var snap in tick.Snapshots)
        {
            var satColor = snap.SaturationLevel < 0.5 ? "green"
                : snap.SaturationLevel < 0.8 ? "yellow" : "red";

            metricsTable.AddRow(
                snap.Name,
                $"{snap.P50LatencyMs:F2}",
                $"{snap.P95LatencyMs:F2}",
                $"{snap.P99LatencyMs:F2}",
                $"{snap.CurrentQueueDepth:N0}",
                $"[{satColor}]{snap.SaturationLevel:P0}[/]",
                $"{snap.TotalProcessed:N0}",
                $"{snap.TotalErrors:N0}"
            );
        }

        AnsiConsole.Write(metricsTable);
        AnsiConsole.WriteLine();

        if (tick.ConsumerMetrics.Count > 0)
        {
            var kafkaTable = new Table()
                .Border(TableBorder.Rounded)
                .Title("[bold]Kafka Consumer Lag[/]")
                .AddColumn("Consumer Group")
                .AddColumn("Lag")
                .AddColumn("Consumed/tick")
                .AddColumn("Latency (ms)");

            foreach (var (group, metrics) in tick.ConsumerMetrics)
            {
                var lagColor = metrics.Lag < 1000 ? "green"
                    : metrics.Lag < 50_000 ? "yellow" : "red";
                kafkaTable.AddRow(
                    group,
                    $"[{lagColor}]{metrics.Lag:N0}[/]",
                    $"{metrics.Consumed:N0}",
                    $"{metrics.LatencyMs:F2}"
                );
            }

            AnsiConsole.Write(kafkaTable);
        }

        var pgSnap = tick.Snapshots.FirstOrDefault(s => s.Name == "PostgreSQL");
        var dbzSnap = tick.Snapshots.FirstOrDefault(s => s.Name == "Debezium CDC");
        var esSnap = tick.Snapshots.FirstOrDefault(s => s.Name == "Elasticsearch");
        var redisSnap = tick.Snapshots.FirstOrDefault(s => s.Name == "Redis");

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold]Pipeline Flow[/]"));
        AnsiConsole.MarkupLine(
            $"  API -> [blue]PG[/] ({pgSnap?.P99LatencyMs:F1}ms) " +
            $"-> [green]CDC[/] (queue:{dbzSnap?.CurrentQueueDepth:N0}) " +
            $"-> [yellow]Kafka[/] (offset:{tick.ConsumerMetrics.Values.Sum(m => m.Lag):N0} lag) " +
            $"-> [cyan]ES[/] (queue:{esSnap?.CurrentQueueDepth:N0}) " +
            $"+ [red]Redis[/] ({redisSnap?.TotalProcessed:N0} keys)"
        );
    }
}
