using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace InventoryCore.Outbox;

public interface IOutboxEntityMapper
{
    /// <summary>Returns null when the entity does not produce outbox rows (e.g. stats, outbox table itself).</summary>
    OutboxMessageDescriptor? Describe(EntityEntry entry);
}

public sealed record OutboxMessageDescriptor(
    string AggregateId,
    string AggregateType,
    string EventType);
