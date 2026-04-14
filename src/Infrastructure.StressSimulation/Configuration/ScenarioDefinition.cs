namespace Infrastructure.StressSimulation.Configuration;

public sealed class ScenarioDefinition
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required TimeSpan Duration { get; init; }
    public double WriteMultiplier { get; init; } = 1.0;
    public double ReadMultiplier { get; init; } = 1.0;
    public FailureInjection? Failure { get; init; }

    public LoadProfile ApplyTo(LoadProfile baseline) => new()
    {
        Branches = baseline.Branches,
        InventoryItems = baseline.InventoryItems,
        Menus = baseline.Menus,
        ConcurrentUsers = (int)(baseline.ConcurrentUsers * Math.Max(WriteMultiplier, ReadMultiplier)),
        AvailabilityTogglesPerMin = baseline.AvailabilityTogglesPerMin * WriteMultiplier,
        PriceUpdatesPerMin = baseline.PriceUpdatesPerMin * WriteMultiplier,
        BranchUpdatesPerMin = baseline.BranchUpdatesPerMin * WriteMultiplier,
        ReadsPerSec = baseline.ReadsPerSec * ReadMultiplier,
        OutboxEventsPerWrite = baseline.OutboxEventsPerWrite
    };

    public static IReadOnlyList<ScenarioDefinition> AllScenarios =>
    [
        new()
        {
            Name = "Normal Production Load",
            Description = "Baseline metrics at specified production rates",
            Duration = TimeSpan.FromMinutes(5)
        },
        new()
        {
            Name = "Black Friday Surge",
            Description = "5x write rate, 10x read rate sustained burst",
            Duration = TimeSpan.FromMinutes(15),
            WriteMultiplier = 5.0,
            ReadMultiplier = 10.0
        },
        new()
        {
            Name = "Branch Bulk Import",
            Description = "1000 branches simultaneously onboarding with 500 items each",
            Duration = TimeSpan.FromMinutes(10),
            WriteMultiplier = 8.0,
            ReadMultiplier = 0.5
        },
        new()
        {
            Name = "Kafka Broker Failure",
            Description = "One of 3 brokers offline for 5 minutes then recovers",
            Duration = TimeSpan.FromMinutes(10),
            Failure = new FailureInjection
            {
                Component = FailureComponent.Kafka,
                ThroughputFactor = 0.33,
                StartsAt = TimeSpan.FromMinutes(2),
                EndsAt = TimeSpan.FromMinutes(7)
            }
        },
        new()
        {
            Name = "Debezium Connector Lag",
            Description = "Connector throughput drops to 20% for 10 minutes",
            Duration = TimeSpan.FromMinutes(15),
            Failure = new FailureInjection
            {
                Component = FailureComponent.Debezium,
                ThroughputFactor = 0.2,
                StartsAt = TimeSpan.FromMinutes(2),
                EndsAt = TimeSpan.FromMinutes(12)
            }
        },
        new()
        {
            Name = "Elasticsearch Slowdown",
            Description = "Indexing throughput drops to 10% for 10 minutes",
            Duration = TimeSpan.FromMinutes(15),
            Failure = new FailureInjection
            {
                Component = FailureComponent.Elasticsearch,
                ThroughputFactor = 0.1,
                StartsAt = TimeSpan.FromMinutes(2),
                EndsAt = TimeSpan.FromMinutes(12)
            }
        },
        new()
        {
            Name = "Redis Memory Saturation",
            Description = "Memory limit hit, eviction begins under sustained load",
            Duration = TimeSpan.FromMinutes(10),
            WriteMultiplier = 3.0,
            Failure = new FailureInjection
            {
                Component = FailureComponent.Redis,
                ThroughputFactor = 0.5,
                StartsAt = TimeSpan.FromMinutes(3),
                EndsAt = TimeSpan.FromMinutes(10)
            }
        }
    ];
}

public sealed class FailureInjection
{
    public required FailureComponent Component { get; init; }
    public required double ThroughputFactor { get; init; }
    public required TimeSpan StartsAt { get; init; }
    public required TimeSpan EndsAt { get; init; }

    public bool IsActiveAt(TimeSpan elapsed) => elapsed >= StartsAt && elapsed < EndsAt;
}

public enum FailureComponent
{
    Kafka,
    Debezium,
    Elasticsearch,
    Redis
}
