using Infrastructure.StressSimulation.Configuration;

namespace Infrastructure.StressSimulation.Simulators;

public sealed class ElasticsearchSimulator : SimulatorBase
{
    private readonly ElasticsearchConfig _cfg;
    private readonly Random _rng = new(45);

    private double _pendingIndexOps;
    private double _totalIndexed;
    private long _versionConflicts;
    private double _throughputFactor = 1.0;
    private double _refreshBacklog;

    public double PendingIndexOps => _pendingIndexOps;
    public double TotalIndexed => _totalIndexed;
    public long VersionConflicts => _versionConflicts;
    public double RefreshBacklog => _refreshBacklog;
    public double EffectiveMaxDocsPerSec => _cfg.MaxIndexDocsPerSec * _throughputFactor;

    public ElasticsearchSimulator(ElasticsearchConfig cfg) : base("Elasticsearch") => _cfg = cfg;

    public void SetThroughputFactor(double factor) => _throughputFactor = Math.Clamp(factor, 0, 1);

    public void EnqueueIndexOps(double count) => _pendingIndexOps += count;

    public (double indexed, double latencyMs) ProcessTick(double elapsedSeconds)
    {
        var maxCanProcess = EffectiveMaxDocsPerSec * elapsedSeconds;
        var toProcess = Math.Min(_pendingIndexOps, maxCanProcess);

        if (toProcess <= 0)
            return (0, 0);

        var bulkCount = Math.Ceiling(toProcess / _cfg.BulkSize);
        var bulkLatencyMs = 5.0 + (_rng.NextDouble() * 10.0);
        if (_pendingIndexOps > maxCanProcess * 2)
            bulkLatencyMs *= 1.5;

        var conflictCount = 0;
        for (var i = 0; i < (int)toProcess; i++)
        {
            if (_rng.NextDouble() < _cfg.VersionConflictProbability)
                conflictCount++;
        }
        _versionConflicts += conflictCount;

        var effectiveIndexed = toProcess - conflictCount;
        _pendingIndexOps -= toProcess;
        _totalIndexed += effectiveIndexed;

        _refreshBacklog += effectiveIndexed;
        var timeSinceRefresh = elapsedSeconds * 1000;
        if (timeSinceRefresh >= _cfg.RefreshIntervalMs)
            _refreshBacklog = 0;

        var totalLatencyMs = bulkCount * bulkLatencyMs;
        var saturation = _pendingIndexOps > 0
            ? Math.Min(1.0, _pendingIndexOps / (EffectiveMaxDocsPerSec * 5))
            : toProcess / maxCanProcess;

        SetQueueDepth(_pendingIndexOps);
        SetSaturation(saturation);
        RecordLatency(totalLatencyMs / Math.Max(1, bulkCount));

        return (effectiveIndexed, totalLatencyMs);
    }

    public double SimulateQuery()
    {
        var baseLatency = 5.0 + (_rng.NextDouble() * 15.0);

        if (_refreshBacklog > 1000)
            baseLatency += Math.Log10(_refreshBacklog) * 5;

        var loadFactor = _pendingIndexOps / Math.Max(1, EffectiveMaxDocsPerSec);
        if (loadFactor > 0.8)
            baseLatency *= 1.0 + (loadFactor - 0.8) * 5;

        return baseLatency;
    }

    public override void Reset()
    {
        ResetCounters();
        _pendingIndexOps = 0;
        _totalIndexed = 0;
        _versionConflicts = 0;
        _throughputFactor = 1.0;
        _refreshBacklog = 0;
    }
}
