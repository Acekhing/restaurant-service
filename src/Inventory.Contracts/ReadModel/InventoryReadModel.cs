namespace Inventory.Contracts.ReadModel;

/// <summary>
/// Denormalized read model combining InventoryItem + InventoryAvailability + owner info.
/// Backed by the <c>inventory_view</c> DB view and used as the ES document shape.
/// </summary>
public class InventoryReadModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ShortName { get; set; }
    public string ItemType { get; set; } = "";
    public string? Tags { get; set; }
    public string? Notes { get; set; }
    public string? Image { get; set; }
    public string? RawImageUrl { get; set; }
    public bool IsOriginalImage { get; set; }
    public decimal DisplayPrice { get; set; }
    public decimal? SupplierPrice { get; set; }
    public decimal? OldSellingPrice { get; set; }
    public decimal DeliveryFee { get; set; }
    public string? PriceRange { get; set; }
    public bool HasDeals { get; set; }
    public string DisplayCurrency { get; set; } = "";
    public bool IsAvailable { get; set; }
    public bool OutOfStock { get; set; }
    public List<InventoryOpeningHours>? OpeningDayHours { get; set; }
    public List<InventoryOpeningHours>? DisplayTimes { get; set; }
    public string RetailerId { get; set; } = "";
    public string RetailerType { get; set; } = "";
    public string? InventoryItemCode { get; set; }
    public bool HasVariety { get; set; }
    public string? Variety { get; set; }
    public string? StationId { get; set; }
    public string? ZoneId { get; set; }
    public int? AveragePreparationTime { get; set; }
}

public class InventoryOpeningHours
{
    public string Id { get; set; } = "";
    public string Day { get; set; } = "";
    public string OpeningTime { get; set; } = "";
    public string ClosingTime { get; set; } = "";
    public bool IsAvailable { get; set; }
}
