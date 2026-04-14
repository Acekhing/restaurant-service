namespace Inventory.API.Data.Entities;

public sealed class InventoryItemPromotion
{
    public string Id { get; set; } = "";
    public string OwnerId { get; set; } = "";
    public int DiscountInPercentage { get; set; }
    public string Currency { get; set; } = "GHS";
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public string? InventoryItemIds { get; set; }
    public string? MenuIds { get; set; }
    public bool IsAppliedToMenu { get; set; }
    public bool IsAppliedToItems { get; set; }
    public bool IsFreeDelivery { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
