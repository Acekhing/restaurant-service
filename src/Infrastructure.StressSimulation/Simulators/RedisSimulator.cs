using Infrastructure.StressSimulation.Configuration;

namespace Infrastructure.StressSimulation.Simulators;

public sealed class RedisSimulator : SimulatorBase
{
    private readonly RedisConfig _cfg;
    private readonly Random _rng = new(46);

    private long _totalKeysCreated;
    private long _totalEvictions;
    private double _currentMemoryBytes;
    private double _throughputFactor = 1.0;

    private readonly Queue<(double createdAtSec, int keyBytes)> _keyRing = new();
    private double _simulationTimeSec;

    public long TotalKeysCreated => _totalKeysCreated;
    public long TotalEvictions => _totalEvictions;
    public double CurrentMemoryMB => _currentMemoryBytes / (1024.0 * 1024.0);
    public double MaxMemoryMB => _cfg.MaxMemoryMB;
    public double MemoryUtilization => CurrentMemoryMB / MaxMemoryMB;
    public long ActiveKeys => _keyRing.Count;

    public RedisSimulator(RedisConfig cfg) : base("Redis") => _cfg = cfg;

    public void SetThroughputFactor(double factor) => _throughputFactor = Math.Clamp(factor, 0, 1);

    public (double latencyMs, bool acquired) SimulateIdempotencyCheck(double currentTimeSec)
    {
        _simulationTimeSec = currentTimeSec;
        ExpireKeys(currentTimeSec);

        var baseLatency = _cfg.BaseLatencyMs;

        if (MemoryUtilization > 0.9)
            baseLatency *= 1.0 + (MemoryUtilization - 0.9) * 20;

        var jitter = _rng.NextDouble() * 0.3;
        var latency = (baseLatency + jitter) / _throughputFactor;

        var isDuplicate = _rng.NextDouble() < 0.001;
        if (isDuplicate)
        {
            RecordLatency(latency);
            return (latency, false);
        }

        var keyBytes = _cfg.MemoryPerKeyBytes;
        _currentMemoryBytes += keyBytes;
        _totalKeysCreated++;

        _keyRing.Enqueue((currentTimeSec, keyBytes));

        if (CurrentMemoryMB > _cfg.MaxMemoryMB)
            Evict();

        SetSaturation(MemoryUtilization);
        RecordLatency(latency);
        return (latency, true);
    }

    private void ExpireKeys(double currentTimeSec)
    {
        var ttlSec = _cfg.IdempotencyTtlHours * 3600.0;
        while (_keyRing.Count > 0 && currentTimeSec - _keyRing.Peek().createdAtSec > ttlSec)
        {
            var (_, bytes) = _keyRing.Dequeue();
            _currentMemoryBytes = Math.Max(0, _currentMemoryBytes - bytes);
        }
    }

    private void Evict()
    {
        while (CurrentMemoryMB > _cfg.MaxMemoryMB * 0.95 && _keyRing.Count > 0)
        {
            var (_, bytes) = _keyRing.Dequeue();
            _currentMemoryBytes = Math.Max(0, _currentMemoryBytes - bytes);
            _totalEvictions++;
        }
        SetQueueDepth(_totalEvictions);
    }

    public override void Reset()
    {
        ResetCounters();
        _totalKeysCreated = 0;
        _totalEvictions = 0;
        _currentMemoryBytes = 0;
        _throughputFactor = 1.0;
        _simulationTimeSec = 0;
        _keyRing.Clear();
    }
}
