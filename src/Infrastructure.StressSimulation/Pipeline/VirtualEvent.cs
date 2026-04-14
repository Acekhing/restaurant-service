namespace Infrastructure.StressSimulation.Pipeline;

public sealed class VirtualEvent
{
    private static long _nextId;

    public long Id { get; } = Interlocked.Increment(ref _nextId);
    public required EventType Type { get; init; }
    public required string AggregateId { get; init; }
    public required string AggregateType { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public double AccumulatedLatencyMs { get; set; }

    public static void ResetIdCounter() => Interlocked.Exchange(ref _nextId, 0);
}

public enum EventType
{
    AvailabilityToggle,
    PriceUpdate,
    ItemUpdate,
    MenuQuery,
    SearchQuery
}
