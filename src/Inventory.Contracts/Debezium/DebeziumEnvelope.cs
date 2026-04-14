namespace Inventory.Contracts.Debezium;

/// <summary>
/// Debezium PostgreSQL connector envelope (JSON) — key fields used by consumers.
/// </summary>
public sealed class DebeziumEnvelope<T>
{
    public DebeziumSource? Source { get; set; }
    public string? Op { get; set; }
    public long? TsMs { get; set; }
    public T? Before { get; set; }
    public T? After { get; set; }
}

public sealed class DebeziumSource
{
    public string? Version { get; set; }
    public string? Connector { get; set; }
    public string? Name { get; set; }
    public long? TsMs { get; set; }
    public string? Db { get; set; }
    public string? Schema { get; set; }
    public string? Table { get; set; }
}
