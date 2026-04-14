using Infrastructure.StressSimulation.Configuration;

namespace Infrastructure.StressSimulation.Simulators;

public sealed class PostgresSimulator : SimulatorBase
{
    private readonly PostgresConfig _cfg;
    private readonly Random _rng = new(42);

    private int _activeConnections;
    private int _queuedRequests;
    private double _walBytesGenerated;
    private double _walBytesReplicated;
    private double _throughputFactor = 1.0;

    public double ActiveConnections => _activeConnections;
    public double QueuedRequests => _queuedRequests;
    public double WalGeneratedMB => _walBytesGenerated / (1024 * 1024);
    public double WalReplicatedMB => _walBytesReplicated / (1024 * 1024);
    public double ReplicationLagMB => Math.Max(0, WalGeneratedMB - WalReplicatedMB);

    public PostgresSimulator(PostgresConfig cfg) : base("PostgreSQL") => _cfg = cfg;

    public void SetThroughputFactor(double factor) => _throughputFactor = Math.Clamp(factor, 0, 1);

    public (double latencyMs, bool success) SimulateWrite(int walRowCount = 1)
    {
        var active = Interlocked.Increment(ref _activeConnections);
        try
        {
            if (active > _cfg.MaxConnections)
            {
                Interlocked.Increment(ref _queuedRequests);
                SetQueueDepth(_queuedRequests);

                var queueWaitMs = 50.0 * (_queuedRequests / (double)_cfg.MaxConnections);
                var totalLatency = queueWaitMs + ComputeWriteLatency(active);

                SetSaturation(Math.Min(1.0, active / (double)_cfg.MaxConnections));

                if (_queuedRequests > _cfg.MaxConnections * 3)
                {
                    RecordError();
                    return (totalLatency, false);
                }

                RecordLatency(totalLatency);
                AddWalBytes(walRowCount);
                return (totalLatency, true);
            }

            var latency = ComputeWriteLatency(active);
            SetSaturation(active / (double)_cfg.MaxConnections);
            RecordLatency(latency);
            AddWalBytes(walRowCount);
            return (latency, true);
        }
        finally
        {
            Interlocked.Decrement(ref _activeConnections);
            if (_queuedRequests > 0)
                Interlocked.Decrement(ref _queuedRequests);
        }
    }

    public (double latencyMs, bool success) SimulateRead()
    {
        var active = Interlocked.Increment(ref _activeConnections);
        try
        {
            if (active > _cfg.MaxConnections)
            {
                Interlocked.Increment(ref _queuedRequests);
                SetQueueDepth(_queuedRequests);
                if (_queuedRequests > _cfg.MaxConnections * 3)
                {
                    RecordError();
                    return (100, false);
                }
            }

            var baseRead = _cfg.BaseWriteLatencyMs * 0.6;
            var contention = active > _cfg.ContentionThreshold
                ? baseRead * Math.Log(1 + (active - _cfg.ContentionThreshold) / (double)_cfg.ContentionThreshold)
                : 0;
            var jitter = _rng.NextDouble() * 0.5;
            var latency = (baseRead + contention + jitter) / _throughputFactor;

            SetSaturation(active / (double)_cfg.MaxConnections);
            RecordLatency(latency);
            return (latency, true);
        }
        finally
        {
            Interlocked.Decrement(ref _activeConnections);
            if (_queuedRequests > 0)
                Interlocked.Decrement(ref _queuedRequests);
        }
    }

    public void AdvanceReplication(double elapsedSeconds)
    {
        var replicationRate = _cfg.ReplicationSlotReadMBps * 1024 * 1024 * elapsedSeconds * _throughputFactor;
        _walBytesReplicated = Math.Min(_walBytesGenerated, _walBytesReplicated + replicationRate);
    }

    private double ComputeWriteLatency(int active)
    {
        var contention = active > _cfg.ContentionThreshold
            ? _cfg.BaseWriteLatencyMs * Math.Log(1 + (active - _cfg.ContentionThreshold) / (double)_cfg.ContentionThreshold)
            : 0;
        var jitter = _rng.NextDouble() * 1.0;
        return (_cfg.BaseWriteLatencyMs + contention + jitter) / _throughputFactor;
    }

    private void AddWalBytes(int rowCount)
    {
        _walBytesGenerated += rowCount * _cfg.WalBytesPerOutboxRow;
    }

    public override void Reset()
    {
        ResetCounters();
        _activeConnections = 0;
        _queuedRequests = 0;
        _walBytesGenerated = 0;
        _walBytesReplicated = 0;
        _throughputFactor = 1.0;
    }
}
