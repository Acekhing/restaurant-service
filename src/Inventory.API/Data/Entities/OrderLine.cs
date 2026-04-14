namespace Inventory.API.Data.Entities;

public sealed class OrderLine
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string InventoryItemId { get; set; } = "";
    public string ItemName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Notes { get; set; }
    public string? VarietySelection { get; set; }
}
