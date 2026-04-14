namespace InventoryCore.Outbox;

public abstract class OutboxEntryBase
{
    public Guid Id { get; set; }
    public string AggregateId { get; set; } = "";
    public string AggregateType { get; set; } = "";
    public string EventType { get; set; } = "";
    public string Payload { get; set; } = "{}";
    public string ActorId { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string? Error { get; set; }
}
