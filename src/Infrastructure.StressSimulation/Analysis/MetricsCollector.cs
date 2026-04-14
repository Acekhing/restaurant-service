using Infrastructure.StressSimulation.Simulators;

namespace Infrastructure.StressSimulation.Analysis;

public sealed class MetricsCollector
{
    private readonly List<MetricsSample> _samples = [];
    private readonly Dictionary<string, List<double>> _latencyHistory = new();
    private readonly Dictionary<string, List<double>> _saturationHistory = new();
    private readonly Dictionary<string, List<double>> _queueHistory = new();

    public IReadOnlyList<MetricsSample> Samples => _samples;

    public void RecordTick(List<SimulatorSnapshot> snapshots, double elapsedSeconds)
    {
        _samples.Add(new MetricsSample
        {
            ElapsedSeconds = elapsedSeconds,
            Snapshots = snapshots
        });

        foreach (var snap in snapshots)
        {
            if (!_latencyHistory.ContainsKey(snap.Name))
            {
                _latencyHistory[snap.Name] = [];
                _saturationHistory[snap.Name] = [];
                _queueHistory[snap.Name] = [];
            }

            _latencyHistory[snap.Name].Add(snap.P99LatencyMs);
            _saturationHistory[snap.Name].Add(snap.SaturationLevel);
            _queueHistory[snap.Name].Add(snap.CurrentQueueDepth);
        }
    }

    public Dictionary<string, ComponentSummary> GetSummaries()
    {
        var result = new Dictionary<string, ComponentSummary>();

        foreach (var name in _latencyHistory.Keys)
        {
            var latencies = _latencyHistory[name];
            var saturations = _saturationHistory[name];
            var queues = _queueHistory[name];

            result[name] = new ComponentSummary
            {
                Name = name,
                AvgP99LatencyMs = latencies.Count > 0 ? latencies.Average() : 0,
                MaxP99LatencyMs = latencies.Count > 0 ? latencies.Max() : 0,
                AvgSaturation = saturations.Count > 0 ? saturations.Average() : 0,
                MaxSaturation = saturations.Count > 0 ? saturations.Max() : 0,
                AvgQueueDepth = queues.Count > 0 ? queues.Average() : 0,
                MaxQueueDepth = queues.Count > 0 ? queues.Max() : 0,
                TotalProcessed = _samples.LastOrDefault()?.Snapshots
                    .FirstOrDefault(s => s.Name == name)?.TotalProcessed ?? 0,
                TotalErrors = _samples.LastOrDefault()?.Snapshots
                    .FirstOrDefault(s => s.Name == name)?.TotalErrors ?? 0
            };
        }

        return result;
    }

    public void Reset()
    {
        _samples.Clear();
        _latencyHistory.Clear();
        _saturationHistory.Clear();
        _queueHistory.Clear();
    }
}

public sealed class MetricsSample
{
    public double ElapsedSeconds { get; init; }
    public List<SimulatorSnapshot> Snapshots { get; init; } = [];
}

public sealed class ComponentSummary
{
    public required string Name { get; init; }
    public double AvgP99LatencyMs { get; init; }
    public double MaxP99LatencyMs { get; init; }
    public double AvgSaturation { get; init; }
    public double MaxSaturation { get; init; }
    public double AvgQueueDepth { get; init; }
    public double MaxQueueDepth { get; init; }
    public long TotalProcessed { get; init; }
    public long TotalErrors { get; init; }
}
