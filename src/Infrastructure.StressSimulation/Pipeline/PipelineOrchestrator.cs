using Infrastructure.StressSimulation.Configuration;
using Infrastructure.StressSimulation.Analysis;
using Infrastructure.StressSimulation.LoadGeneration;
using Infrastructure.StressSimulation.Simulators;

namespace Infrastructure.StressSimulation.Pipeline;

public sealed class PipelineOrchestrator
{
    private readonly SimulationConfig _config;
    private readonly ApiLoadGenerator _apiGen;
    private readonly PostgresSimulator _postgres;
    private readonly DebeziumCdcSimulator _debezium;
    private readonly KafkaSimulator _kafka;
    private readonly ElasticsearchSimulator _elasticsearch;
    private readonly RedisSimulator _redis;
    private readonly MetricsCollector _metrics;
    private readonly BackpressureDetector _backpressure;

    public ApiLoadGenerator ApiGen => _apiGen;
    public PostgresSimulator Postgres => _postgres;
    public DebeziumCdcSimulator Debezium => _debezium;
    public KafkaSimulator Kafka => _kafka;
    public ElasticsearchSimulator Elasticsearch => _elasticsearch;
    public RedisSimulator Redis => _redis;
    public MetricsCollector Metrics => _metrics;
    public BackpressureDetector Backpressure => _backpressure;

    public PipelineOrchestrator(SimulationConfig config)
    {
        _config = config;
        _apiGen = new ApiLoadGenerator();
        _postgres = new PostgresSimulator(config.Postgres);
        _debezium = new DebeziumCdcSimulator(config.Debezium);
        _kafka = new KafkaSimulator(config.Kafka);
        _elasticsearch = new ElasticsearchSimulator(config.Elasticsearch);
        _redis = new RedisSimulator(config.Redis);
        _metrics = new MetricsCollector();
        _backpressure = new BackpressureDetector();
    }

    public IReadOnlyList<SimulatorBase> AllSimulators =>
        [_apiGen, _postgres, _debezium, _kafka, _elasticsearch, _redis];

    public void ApplyFailure(FailureInjection? failure, TimeSpan elapsed)
    {
        if (failure is null) return;

        var isActive = failure.IsActiveAt(elapsed);
        var factor = isActive ? failure.ThroughputFactor : 1.0;

        switch (failure.Component)
        {
            case FailureComponent.Kafka:
                _kafka.SetThroughputFactor(factor);
                break;
            case FailureComponent.Debezium:
                _debezium.SetThroughputFactor(factor);
                break;
            case FailureComponent.Elasticsearch:
                _elasticsearch.SetThroughputFactor(factor);
                break;
            case FailureComponent.Redis:
                _redis.SetThroughputFactor(factor);
                break;
        }
    }

    public TickResult RunTick(LoadProfile profile, double tickSeconds, double totalElapsedSeconds)
    {
        var batch = _apiGen.GenerateTickBatch(profile, tickSeconds);

        // --- Write path: aggregate simulation ---
        var writeCount = batch.TotalWrites;
        var writeErrors = 0;
        double totalWriteLatency = 0;

        // Simulate writes in statistical batches (sample up to 500 to model distribution)
        var writeSamples = Math.Min(writeCount, 500);
        var sampleWriteLatencySum = 0.0;
        var sampleWriteErrors = 0;

        for (var i = 0; i < writeSamples; i++)
        {
            var outboxRowCount = (int)Math.Ceiling(profile.OutboxEventsPerWrite);
            var (pgLatency, pgSuccess) = _postgres.SimulateWrite(outboxRowCount);

            if (!pgSuccess)
            {
                sampleWriteErrors++;
                continue;
            }
            sampleWriteLatencySum += pgLatency;
        }

        var avgWriteLatency = writeSamples > sampleWriteErrors
            ? sampleWriteLatencySum / (writeSamples - sampleWriteErrors)
            : 50.0;
        var errorRate = writeSamples > 0 ? (double)sampleWriteErrors / writeSamples : 0;

        writeErrors = (int)(writeCount * errorRate);
        totalWriteLatency = avgWriteLatency * (writeCount - writeErrors);
        _apiGen.RecordBatchLatency(avgWriteLatency, writeCount - writeErrors, writeErrors);

        // --- Read path: aggregate simulation ---
        var readCount = batch.TotalReads;
        var readSamples = (int)Math.Min(readCount, 200);
        var sampleReadLatencySum = 0.0;

        for (var i = 0; i < readSamples; i++)
        {
            sampleReadLatencySum += _elasticsearch.SimulateQuery();
        }

        var avgReadLatency = readSamples > 0 ? sampleReadLatencySum / readSamples : 10.0;
        _apiGen.RecordBatchLatency(avgReadLatency, readSamples, 0);

        // --- CDC pipeline ---
        var outboxEventsGenerated = (writeCount - writeErrors) * profile.OutboxEventsPerWrite;

        _debezium.IngestWalEvents(outboxEventsGenerated);
        _postgres.AdvanceReplication(tickSeconds);
        var (debeziumProcessed, _) = _debezium.ProcessTick(tickSeconds);

        _kafka.ProduceEvents(debeziumProcessed);
        _kafka.MaybeRandomRebalance(totalElapsedSeconds);
        var consumerMetrics = _kafka.ConsumeEvents(tickSeconds);

        // Each consumed event fans out to ES (elastic projector) and all to Redis.
        // ~30% inventory items, ~5% menus, ~1% branches → ~36% total go to ES indexing.
        var esEventsToIndex = 0.0;
        var redisChecks = 0.0;
        foreach (var (_, cm) in consumerMetrics)
        {
            esEventsToIndex += cm.Consumed * 0.36;
            redisChecks += cm.Consumed;
        }

        _elasticsearch.EnqueueIndexOps(esEventsToIndex);
        _elasticsearch.ProcessTick(tickSeconds);

        // Redis: simulate in aggregate (sample up to 200)
        var redisSampleCount = (int)Math.Min(redisChecks, 200);
        for (var i = 0; i < redisSampleCount; i++)
            _redis.SimulateIdempotencyCheck(totalElapsedSeconds);

        // --- Metrics collection ---
        var snapshots = AllSimulators.Select(s => s.TakeSnapshot()).ToList();
        _metrics.RecordTick(snapshots, totalElapsedSeconds);
        _backpressure.Evaluate(snapshots, consumerMetrics);

        return new TickResult
        {
            WriteCount = writeCount - writeErrors,
            ReadCount = readCount,
            WriteErrors = writeErrors,
            AvgWriteLatencyMs = avgWriteLatency,
            AvgReadLatencyMs = avgReadLatency,
            OutboxEventsGenerated = outboxEventsGenerated,
            DebeziumProcessed = debeziumProcessed,
            ConsumerMetrics = consumerMetrics,
            BackpressureLevel = _backpressure.CurrentLevel,
            Snapshots = snapshots
        };
    }

    public void Reset()
    {
        VirtualEvent.ResetIdCounter();
        foreach (var sim in AllSimulators) sim.Reset();
        _metrics.Reset();
        _backpressure.Reset();
    }
}

public sealed class TickResult
{
    public int WriteCount { get; init; }
    public long ReadCount { get; init; }
    public int WriteErrors { get; init; }
    public double AvgWriteLatencyMs { get; init; }
    public double AvgReadLatencyMs { get; init; }
    public double OutboxEventsGenerated { get; init; }
    public double DebeziumProcessed { get; init; }
    public Dictionary<string, ConsumerMetrics> ConsumerMetrics { get; init; } = new();
    public double BackpressureLevel { get; init; }
    public List<SimulatorSnapshot> Snapshots { get; init; } = [];
}
