using Infrastructure.StressSimulation.Configuration;

namespace Infrastructure.StressSimulation.Simulators;

public sealed class KafkaSimulator : SimulatorBase
{
    private readonly KafkaConfig _cfg;
    private readonly Random _rng = new(44);
    private readonly Dictionary<string, ConsumerGroupState> _consumerGroups = new();

    private long _topicOffset;
    private double _throughputFactor = 1.0;
    private int _activeBrokers;
    private double _lastRebalanceTime;

    public long TopicOffset => _topicOffset;
    public int ActiveBrokers => _activeBrokers;

    public KafkaSimulator(KafkaConfig cfg) : base("Kafka")
    {
        _cfg = cfg;
        _activeBrokers = cfg.BrokerCount;
        foreach (var group in cfg.ConsumerGroups)
            _consumerGroups[group] = new ConsumerGroupState();
    }

    public void SetThroughputFactor(double factor)
    {
        _throughputFactor = Math.Clamp(factor, 0, 1);
        _activeBrokers = Math.Max(1, (int)(_cfg.BrokerCount * _throughputFactor));
    }

    public double ProduceEvents(double eventCount)
    {
        var maxPerSec = _cfg.MaxBrokerWriteMBps * 1024 * 1024 / 500.0 * _activeBrokers;
        var produced = Math.Min(eventCount, maxPerSec);
        _topicOffset += (long)produced;

        var saturation = produced / maxPerSec;
        SetSaturation(saturation);

        var latencyMs = 1.0 + (_rng.NextDouble() * 2.0) + (saturation > 0.8 ? (saturation - 0.8) * 50 : 0);
        RecordLatency(latencyMs);

        return produced;
    }

    public Dictionary<string, ConsumerMetrics> ConsumeEvents(double elapsedSeconds)
    {
        var result = new Dictionary<string, ConsumerMetrics>();
        var maxConsumeRate = _cfg.MaxBrokerWriteMBps * 1024 * 1024 / 500.0 * _activeBrokers * 0.8;

        foreach (var (groupName, state) in _consumerGroups)
        {
            var lag = _topicOffset - state.CommittedOffset;
            if (lag <= 0)
            {
                result[groupName] = new ConsumerMetrics { Lag = 0, Consumed = 0, LatencyMs = 0 };
                continue;
            }

            var batchSize = Math.Min(lag, maxConsumeRate * elapsedSeconds / _cfg.ConsumerGroups.Length);
            if (state.IsRebalancing)
            {
                batchSize *= 0.1;
                state.RebalanceRemainingMs -= elapsedSeconds * 1000;
                if (state.RebalanceRemainingMs <= 0)
                    state.IsRebalancing = false;
            }

            batchSize *= _throughputFactor;
            state.CommittedOffset += (long)batchSize;

            var consumeLatency = 0.5 + (_rng.NextDouble() * 1.0);
            if (lag > 10_000) consumeLatency += Math.Log10(lag) * 2;

            var finalLag = _topicOffset - state.CommittedOffset;
            result[groupName] = new ConsumerMetrics
            {
                Lag = finalLag,
                Consumed = (long)batchSize,
                LatencyMs = consumeLatency
            };
        }

        var totalLag = result.Values.Sum(m => m.Lag);
        SetQueueDepth(totalLag);

        return result;
    }

    public void TriggerRebalance(string groupName, double durationMs = 5000)
    {
        if (_consumerGroups.TryGetValue(groupName, out var state))
        {
            state.IsRebalancing = true;
            state.RebalanceRemainingMs = durationMs;
        }
    }

    public void MaybeRandomRebalance(double elapsedTotalSeconds)
    {
        if (elapsedTotalSeconds - _lastRebalanceTime > 60 && _rng.NextDouble() < 0.05)
        {
            var group = _cfg.ConsumerGroups[_rng.Next(_cfg.ConsumerGroups.Length)];
            TriggerRebalance(group, 3000);
            _lastRebalanceTime = elapsedTotalSeconds;
        }
    }

    public override void Reset()
    {
        ResetCounters();
        _topicOffset = 0;
        _throughputFactor = 1.0;
        _activeBrokers = _cfg.BrokerCount;
        _lastRebalanceTime = 0;
        foreach (var state in _consumerGroups.Values)
        {
            state.CommittedOffset = 0;
            state.IsRebalancing = false;
            state.RebalanceRemainingMs = 0;
        }
    }
}

public sealed class ConsumerGroupState
{
    public long CommittedOffset { get; set; }
    public bool IsRebalancing { get; set; }
    public double RebalanceRemainingMs { get; set; }
}

public sealed class ConsumerMetrics
{
    public long Lag { get; init; }
    public long Consumed { get; init; }
    public double LatencyMs { get; init; }
}
