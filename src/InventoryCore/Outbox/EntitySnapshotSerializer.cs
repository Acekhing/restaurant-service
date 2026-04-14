using System.Text.Json;
using Inventory.Contracts.Outbox;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace InventoryCore.Outbox;

public static class EntitySnapshotSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    public static Dictionary<string, object?> Snapshot(PropertyValues values)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in values.Properties)
        {
            if (prop.IsPrimaryKey())
                continue;
            var name = prop.Name;
            dict[name] = Sanitize(values[name]);
        }

        return dict;
    }

    public static string SerializePayload(UnifiedAuditPayload payload) =>
        JsonSerializer.Serialize(payload, JsonOptions);

    private static object? Sanitize(object? v)
    {
        if (v is DateTime dt)
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        if (v is DateTimeOffset dto)
            return dto;
        return v;
    }
}
