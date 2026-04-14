namespace Infrastructure.StressSimulation.Configuration;

public sealed class SimulationConfig
{
    public PostgresConfig Postgres { get; init; } = new();
    public DebeziumConfig Debezium { get; init; } = new();
    public KafkaConfig Kafka { get; init; } = new();
    public ElasticsearchConfig Elasticsearch { get; init; } = new();
    public RedisConfig Redis { get; init; } = new();
}

public sealed class PostgresConfig
{
    public int MaxConnections { get; init; } = 100;
    public double BaseWriteLatencyMs { get; init; } = 2.0;
    public int ContentionThreshold { get; init; } = 50;
    public int WalBytesPerOutboxRow { get; init; } = 512;
    public double ReplicationSlotReadMBps { get; init; } = 50;
}

public sealed class DebeziumConfig
{
    public double MaxEventsPerSecond { get; init; } = 10_000;
    public int Tasks { get; init; } = 1;
    public double SerializationCostMs { get; init; } = 0.1;
    public int MaxWalSenders { get; init; } = 16;
}

public sealed class KafkaConfig
{
    public int Partitions { get; init; } = 6;
    public int BrokerCount { get; init; } = 3;
    public double MaxBrokerWriteMBps { get; init; } = 100;
    public string[] ConsumerGroups { get; init; } =
    [
        "inventory-elastic-projector",
        "inventory-audit-worker"
    ];
    public int FetchMinBytes { get; init; } = 1_048_576;
    public int FetchMaxWaitMs { get; init; } = 500;
}

public sealed class ElasticsearchConfig
{
    public double MaxIndexDocsPerSec { get; init; } = 5_000;
    public int BulkSize { get; init; } = 50;
    public int RefreshIntervalMs { get; init; } = 1000;
    public long InventoryDocCount { get; init; } = 10_000_000;
    public long MenuDocCount { get; init; } = 5_000_000;
    public long BranchDocCount { get; init; } = 30_000;
    public double VersionConflictProbability { get; init; } = 0.001;
}

public sealed class RedisConfig
{
    public long MaxMemoryMB { get; init; } = 512;
    public int MemoryPerKeyBytes { get; init; } = 100;
    public int IdempotencyTtlHours { get; init; } = 24;
    public int EffectiveDeduplicationWindowMinutes { get; init; } = 60;
    public string EvictionPolicy { get; init; } = "allkeys-lru";
    public double BaseLatencyMs { get; init; } = 0.2;
}
