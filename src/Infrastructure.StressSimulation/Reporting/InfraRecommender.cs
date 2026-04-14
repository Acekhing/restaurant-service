using Infrastructure.StressSimulation.Analysis;
using Infrastructure.StressSimulation.Configuration;

namespace Infrastructure.StressSimulation.Reporting;

public static class InfraRecommender
{
    public static List<InfraRecommendation> Recommend(
        LoadProfile profile,
        SimulationConfig config,
        Dictionary<string, ComponentSummary> summaries,
        List<ScalingForecast> forecasts)
    {
        var recs = new List<InfraRecommendation>();

        RecommendKafka(profile, config, summaries, recs);
        RecommendDebezium(profile, config, summaries, recs);
        RecommendPostgres(profile, config, summaries, recs);
        RecommendRedis(profile, config, summaries, recs);
        RecommendElasticsearch(profile, config, summaries, recs);

        return recs;
    }

    private static void RecommendKafka(
        LoadProfile profile, SimulationConfig config,
        Dictionary<string, ComponentSummary> summaries,
        List<InfraRecommendation> recs)
    {
        var eventsPerSec = profile.TotalOutboxEventsPerSec;
        var currentPartitions = config.Kafka.Partitions;
        var eventsPerPartition = eventsPerSec / currentPartitions;

        var recommendedPartitions = (int)Math.Ceiling(eventsPerSec / 1500.0);
        recommendedPartitions = Math.Max(config.Kafka.ConsumerGroups.Length, recommendedPartitions);
        recommendedPartitions = RoundToNice(recommendedPartitions);

        recs.Add(new InfraRecommendation
        {
            Component = "Kafka",
            Setting = "Partition Count",
            CurrentValue = currentPartitions.ToString(),
            RecommendedValue = recommendedPartitions.ToString(),
            Reason = $"At {eventsPerSec:N0} events/sec, {eventsPerPartition:N0} events/sec/partition. " +
                     $"Target ~1,500 events/sec/partition for headroom."
        });

        var brokers = Math.Max(3, (int)Math.Ceiling(recommendedPartitions / 10.0));
        recs.Add(new InfraRecommendation
        {
            Component = "Kafka",
            Setting = "Broker Count",
            CurrentValue = config.Kafka.BrokerCount.ToString(),
            RecommendedValue = brokers.ToString(),
            Reason = $"~10 partitions per broker for balanced replication."
        });
    }

    private static void RecommendDebezium(
        LoadProfile profile, SimulationConfig config,
        Dictionary<string, ComponentSummary> summaries,
        List<InfraRecommendation> recs)
    {
        var eventsPerSec = profile.TotalOutboxEventsPerSec;
        var currentCapacity = config.Debezium.MaxEventsPerSecond * config.Debezium.Tasks;
        var tasks = (int)Math.Ceiling(eventsPerSec / (config.Debezium.MaxEventsPerSecond * 0.7));
        tasks = Math.Max(1, tasks);

        recs.Add(new InfraRecommendation
        {
            Component = "Debezium",
            Setting = "Connector Tasks",
            CurrentValue = config.Debezium.Tasks.ToString(),
            RecommendedValue = tasks.ToString(),
            Reason = $"Need {eventsPerSec:N0} events/sec capacity. " +
                     $"Each task handles ~{config.Debezium.MaxEventsPerSecond:N0} events/sec at 70% target utilization."
        });
    }

    private static void RecommendPostgres(
        LoadProfile profile, SimulationConfig config,
        Dictionary<string, ComponentSummary> summaries,
        List<InfraRecommendation> recs)
    {
        var writesPerSec = profile.TotalWritesPerSec;
        var avgLatencyMs = summaries.TryGetValue("PostgreSQL", out var pg) ? pg.AvgP99LatencyMs : 5.0;
        var connectionsNeeded = (int)Math.Ceiling(writesPerSec * (avgLatencyMs / 1000.0) * 1.5);
        connectionsNeeded = Math.Max(25, Math.Min(500, connectionsNeeded));

        recs.Add(new InfraRecommendation
        {
            Component = "PostgreSQL",
            Setting = "Connection Pool Size",
            CurrentValue = config.Postgres.MaxConnections.ToString(),
            RecommendedValue = connectionsNeeded.ToString(),
            Reason = $"At {writesPerSec:N0} writes/sec with ~{avgLatencyMs:F1}ms P99 latency, " +
                     $"need {connectionsNeeded} connections (1.5x headroom)."
        });

        var walMBPerSec = writesPerSec * profile.OutboxEventsPerWrite * config.Postgres.WalBytesPerOutboxRow / (1024.0 * 1024.0);
        var walSenderSlots = Math.Max(4, (int)Math.Ceiling(walMBPerSec / config.Postgres.ReplicationSlotReadMBps * 2));
        recs.Add(new InfraRecommendation
        {
            Component = "PostgreSQL",
            Setting = "max_wal_senders",
            CurrentValue = "16",
            RecommendedValue = walSenderSlots.ToString(),
            Reason = $"WAL generation rate ~{walMBPerSec:F1} MB/sec. Need sufficient wal senders for replication."
        });
    }

    private static void RecommendRedis(
        LoadProfile profile, SimulationConfig config,
        Dictionary<string, ComponentSummary> summaries,
        List<InfraRecommendation> recs)
    {
        var totalEventsPerSec = profile.TotalOutboxEventsPerSec;
        var consumerCount = config.Kafka.ConsumerGroups.Length;
        var keysPerSec = totalEventsPerSec * consumerCount;

        var usesLruEviction = config.Redis.EvictionPolicy.Contains("lru", StringComparison.OrdinalIgnoreCase)
                           || config.Redis.EvictionPolicy.Contains("lfu", StringComparison.OrdinalIgnoreCase);

        double sizingWindowSeconds;
        string windowDescription;

        if (usesLruEviction)
        {
            sizingWindowSeconds = config.Redis.EffectiveDeduplicationWindowMinutes * 60.0;
            windowDescription = $"{config.Redis.EffectiveDeduplicationWindowMinutes}m dedup window ({config.Redis.EvictionPolicy} eviction)";
        }
        else
        {
            sizingWindowSeconds = config.Redis.IdempotencyTtlHours * 3600.0;
            windowDescription = $"{config.Redis.IdempotencyTtlHours}h TTL (no eviction)";
        }

        var peakKeyCount = (long)(keysPerSec * sizingWindowSeconds);
        var memoryNeededMB = peakKeyCount * config.Redis.MemoryPerKeyBytes / (1024.0 * 1024.0);
        var recommendedMB = Math.Max(256, (long)Math.Ceiling(memoryNeededMB * 1.3));

        recs.Add(new InfraRecommendation
        {
            Component = "Redis",
            Setting = "Max Memory",
            CurrentValue = $"{config.Redis.MaxMemoryMB} MB",
            RecommendedValue = $"{recommendedMB} MB",
            Reason = $"At {keysPerSec:N0} keys/sec across {consumerCount} consumers with {windowDescription}, " +
                     $"peak key count ~{peakKeyCount:N0} consuming ~{memoryNeededMB:N0} MB (1.3x headroom)."
        });
    }

    private static void RecommendElasticsearch(
        LoadProfile profile, SimulationConfig config,
        Dictionary<string, ComponentSummary> summaries,
        List<InfraRecommendation> recs)
    {
        var totalDocs = config.Elasticsearch.InventoryDocCount
                     + config.Elasticsearch.MenuDocCount
                     + config.Elasticsearch.BranchDocCount;
        var shardSizeGB = 30;
        var avgDocSizeKB = 2;
        var totalSizeGB = totalDocs * avgDocSizeKB / (1024.0 * 1024.0);
        var shards = (int)Math.Ceiling(totalSizeGB / shardSizeGB);
        shards = Math.Max(2, shards);

        recs.Add(new InfraRecommendation
        {
            Component = "Elasticsearch",
            Setting = "Primary Shards (across all indices)",
            CurrentValue = "5 (default)",
            RecommendedValue = shards.ToString(),
            Reason = $"Total estimated data size: {totalSizeGB:F1} GB across {totalDocs:N0} docs (inventory + menus + branches). " +
                     $"Target ~{shardSizeGB} GB per shard."
        });

        var indexRate = profile.TotalOutboxEventsPerSec;
        var nodesNeeded = Math.Max(3, (int)Math.Ceiling(indexRate / config.Elasticsearch.MaxIndexDocsPerSec));
        recs.Add(new InfraRecommendation
        {
            Component = "Elasticsearch",
            Setting = "Data Nodes",
            CurrentValue = "1",
            RecommendedValue = nodesNeeded.ToString(),
            Reason = $"At {indexRate:N0} index ops/sec, each node handles ~{config.Elasticsearch.MaxIndexDocsPerSec:N0}/sec."
        });

        var refreshInterval = indexRate > 5000 ? 5000 : config.Elasticsearch.RefreshIntervalMs;
        recs.Add(new InfraRecommendation
        {
            Component = "Elasticsearch",
            Setting = "refresh_interval",
            CurrentValue = $"{config.Elasticsearch.RefreshIntervalMs}ms",
            RecommendedValue = $"{refreshInterval}ms",
            Reason = indexRate > 5000
                ? "High index rate benefits from longer refresh intervals to reduce I/O."
                : "Current refresh interval is adequate."
        });
    }

    private static int RoundToNice(int n)
    {
        if (n <= 6) return 6;
        if (n <= 12) return 12;
        if (n <= 24) return 24;
        return (int)(Math.Ceiling(n / 12.0) * 12);
    }
}

public sealed class InfraRecommendation
{
    public required string Component { get; init; }
    public required string Setting { get; init; }
    public required string CurrentValue { get; init; }
    public required string RecommendedValue { get; init; }
    public required string Reason { get; init; }
}
