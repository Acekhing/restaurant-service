using Infrastructure.StressSimulation.Configuration;

namespace Infrastructure.StressSimulation.Simulators;

public sealed class DebeziumCdcSimulator : SimulatorBase
{
    private readonly DebeziumConfig _cfg;
    private readonly Random _rng = new(43);

    private double _walBacklog;
    private double _totalEventsProduced;
    private double _throughputFactor = 1.0;

    public double WalBacklog => _walBacklog;
    public double TotalEventsProduced => _totalEventsProduced;
    public double EffectiveMaxEventsPerSec => _cfg.MaxEventsPerSecond * _cfg.Tasks * _throughputFactor;
    public double ReplicationSlotLagEvents => _walBacklog;

    public DebeziumCdcSimulator(DebeziumConfig cfg) : base("Debezium CDC") => _cfg = cfg;

    public void SetThroughputFactor(double factor) => _throughputFactor = Math.Clamp(factor, 0, 1);

    public void IngestWalEvents(double eventCount)
    {
        _walBacklog += eventCount;
        SetQueueDepth(_walBacklog);
    }

    public (double eventsProcessed, double latencyMs) ProcessTick(double elapsedSeconds)
    {
        var maxCanProcess = EffectiveMaxEventsPerSec * elapsedSeconds;
        var toProcess = Math.Min(_walBacklog, maxCanProcess);

        if (toProcess <= 0)
            return (0, 0);

        var serializationMs = toProcess * _cfg.SerializationCostMs;
        var networkJitter = _rng.NextDouble() * 2.0;
        var totalLatency = serializationMs + networkJitter;

        _walBacklog -= toProcess;
        _totalEventsProduced += toProcess;

        var saturation = _walBacklog > 0
            ? Math.Min(1.0, _walBacklog / (EffectiveMaxEventsPerSec * 10))
            : toProcess / maxCanProcess;

        SetQueueDepth(_walBacklog);
        SetSaturation(saturation);
        RecordLatency(totalLatency / Math.Max(1, toProcess));

        return (toProcess, totalLatency);
    }

    public override void Reset()
    {
        ResetCounters();
        _walBacklog = 0;
        _totalEventsProduced = 0;
        _throughputFactor = 1.0;
    }
}
