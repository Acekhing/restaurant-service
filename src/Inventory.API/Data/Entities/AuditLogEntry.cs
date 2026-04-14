namespace Inventory.API.Data.Entities;

public sealed class AuditLogEntry
{
    public Guid Id { get; set; }
    public Guid OutboxId { get; set; }
    public string AggregateId { get; set; } = "";
    public string AggregateType { get; set; } = "";
    public string EventType { get; set; } = "";
    public string ActorId { get; set; } = "";
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
}

