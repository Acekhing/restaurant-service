using System.Diagnostics;

namespace Infrastructure.StressSimulation.Simulators;

public abstract class SimulatorBase
{
    private readonly object _lock = new();
    private readonly List<double> _latencySamples = new(capacity: 100_000);
    private long _totalProcessed;
    private long _totalErrors;
    private double _currentQueueDepth;
    private double _peakQueueDepth;
    private double _saturationLevel;

    public string Name { get; }

    protected SimulatorBase(string name) => Name = name;

    public SimulatorSnapshot TakeSnapshot()
    {
        lock (_lock)
        {
            var sorted = _latencySamples.Count > 0
                ? _latencySamples.OrderBy(x => x).ToList()
                : [];

            var snapshot = new SimulatorSnapshot
            {
                Name = Name,
                TotalProcessed = _totalProcessed,
                TotalErrors = _totalErrors,
                CurrentQueueDepth = _currentQueueDepth,
                PeakQueueDepth = _peakQueueDepth,
                SaturationLevel = _saturationLevel,
                P50LatencyMs = Percentile(sorted, 0.50),
                P95LatencyMs = Percentile(sorted, 0.95),
                P99LatencyMs = Percentile(sorted, 0.99),
                AvgLatencyMs = sorted.Count > 0 ? sorted.Average() : 0,
                SampleCount = sorted.Count
            };

            _latencySamples.Clear();
            return snapshot;
        }
    }

    protected void RecordLatency(double ms)
    {
        lock (_lock)
        {
            _latencySamples.Add(ms);
            _totalProcessed++;
        }
    }

    protected void RecordError()
    {
        lock (_lock) { _totalErrors++; }
    }

    protected void SetQueueDepth(double depth)
    {
        lock (_lock)
        {
            _currentQueueDepth = depth;
            if (depth > _peakQueueDepth) _peakQueueDepth = depth;
        }
    }

    protected void SetSaturation(double level)
    {
        lock (_lock) { _saturationLevel = Math.Clamp(level, 0, 1); }
    }

    public abstract void Reset();

    protected void ResetCounters()
    {
        lock (_lock)
        {
            _latencySamples.Clear();
            _totalProcessed = 0;
            _totalErrors = 0;
            _currentQueueDepth = 0;
            _peakQueueDepth = 0;
            _saturationLevel = 0;
        }
    }

    private static double Percentile(List<double> sorted, double p)
    {
        if (sorted.Count == 0) return 0;
        var idx = (int)Math.Ceiling(p * sorted.Count) - 1;
        return sorted[Math.Clamp(idx, 0, sorted.Count - 1)];
    }
}

public sealed class SimulatorSnapshot
{
    public required string Name { get; init; }
    public long TotalProcessed { get; init; }
    public long TotalErrors { get; init; }
    public double CurrentQueueDepth { get; init; }
    public double PeakQueueDepth { get; init; }
    public double SaturationLevel { get; init; }
    public double P50LatencyMs { get; init; }
    public double P95LatencyMs { get; init; }
    public double P99LatencyMs { get; init; }
    public double AvgLatencyMs { get; init; }
    public int SampleCount { get; init; }
}
