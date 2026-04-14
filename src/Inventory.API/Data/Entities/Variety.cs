namespace Inventory.API.Data.Entities;

public sealed class Variety
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string InventoryItemIds { get; set; } = "[]";
    public List<VarietyData> Varieties { get; set; } = new();
    public string OwnerId { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class VarietyData
{
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}
