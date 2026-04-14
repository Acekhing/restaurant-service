namespace Inventory.Contracts.Outbox;

/// <summary>
/// Unified audit payload stored in inventory_outbox.payload (JSONB) and projected to Elasticsearch.
/// </summary>
public sealed class UnifiedAuditPayload
{
    public required string Actor { get; init; }
    public required string AggregateId { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public Dictionary<string, object?>? Before { get; init; }
    public Dictionary<string, object?>? After { get; init; }
}

/// <summary>
/// Denormalized shape for Elasticsearch documents (built by ElasticProjector).
/// </summary>
public sealed class InventorySearchDocument
{
    public required string Id { get; init; }
    public required string Vertical { get; init; }
    public required string AggregateType { get; init; }
    public required string EventType { get; init; }
    public required string AggregateId { get; init; }
    public required long RowVersion { get; init; }
    public required UnifiedAuditPayload Audit { get; init; }
}
