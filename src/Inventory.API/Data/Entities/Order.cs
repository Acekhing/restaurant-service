namespace Inventory.API.Data.Entities;

public sealed class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public string RetailerId { get; set; } = "";
    public string? WaiterName { get; set; }
    public string? TableNumber { get; set; }
    public string? CustomerNotes { get; set; }
    public string? CustomerPhone { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public string DisplayCurrency { get; set; } = "GHS";
    public List<OrderLine> Lines { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
