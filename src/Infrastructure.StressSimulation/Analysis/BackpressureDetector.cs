using Infrastructure.StressSimulation.Simulators;

namespace Infrastructure.StressSimulation.Analysis;

public sealed class BackpressureDetector
{
    private readonly List<BackpressureAlert> _alerts = [];
    private double _currentLevel;

    public double CurrentLevel => _currentLevel;
    public IReadOnlyList<BackpressureAlert> Alerts => _alerts;

    public void Evaluate(
        List<SimulatorSnapshot> snapshots,
        Dictionary<string, ConsumerMetrics> consumerMetrics)
    {
        var signals = new List<double>();

        foreach (var snap in snapshots)
        {
            if (snap.SaturationLevel > 0.85)
            {
                signals.Add(snap.SaturationLevel);
                if (snap.SaturationLevel > 0.95)
                    AddAlert(BackpressureAlertType.ServiceSaturation, snap.Name,
                        $"{snap.Name} saturation at {snap.SaturationLevel:P0}");
            }

            if (snap.CurrentQueueDepth > 10_000)
            {
                signals.Add(Math.Min(1.0, snap.CurrentQueueDepth / 100_000));
                AddAlert(BackpressureAlertType.QueueBuildup, snap.Name,
                    $"{snap.Name} queue depth: {snap.CurrentQueueDepth:N0}");
            }

            if (snap.P99LatencyMs > 500)
            {
                signals.Add(Math.Min(1.0, snap.P99LatencyMs / 2000));
                AddAlert(BackpressureAlertType.CascadingLatency, snap.Name,
                    $"{snap.Name} P99 latency: {snap.P99LatencyMs:F1}ms");
            }
        }

        foreach (var (group, metrics) in consumerMetrics)
        {
            if (metrics.Lag > 50_000)
            {
                signals.Add(Math.Min(1.0, metrics.Lag / 500_000.0));
                AddAlert(BackpressureAlertType.ConsumerLagExplosion, group,
                    $"Consumer group '{group}' lag: {metrics.Lag:N0}");
            }
        }

        var kafkaSnapshot = snapshots.FirstOrDefault(s => s.Name == "Kafka");
        var pgSnapshot = snapshots.FirstOrDefault(s => s.Name == "PostgreSQL");
        if (kafkaSnapshot is not null && pgSnapshot is not null
            && kafkaSnapshot.SaturationLevel > 0.9 && pgSnapshot.SaturationLevel > 0.8)
        {
            signals.Add(0.95);
            AddAlert(BackpressureAlertType.ThunderingHerd, "System",
                "Both Kafka and PostgreSQL near saturation - thundering herd risk");
        }

        _currentLevel = signals.Count > 0 ? signals.Average() : 0;
    }

    private void AddAlert(BackpressureAlertType type, string component, string message)
    {
        if (_alerts.Count > 500)
            _alerts.RemoveRange(0, 100);

        _alerts.Add(new BackpressureAlert
        {
            Type = type,
            Component = component,
            Message = message,
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    public void Reset()
    {
        _alerts.Clear();
        _currentLevel = 0;
    }
}

public sealed class BackpressureAlert
{
    public required BackpressureAlertType Type { get; init; }
    public required string Component { get; init; }
    public required string Message { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}

public enum BackpressureAlertType
{
    QueueBuildup,
    CascadingLatency,
    ServiceSaturation,
    ConsumerLagExplosion,
    ThunderingHerd
}
