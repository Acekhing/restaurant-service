namespace Inventory.Contracts.ReadModel;

/// <summary>
/// Denormalized read model combining Menu + owner info + inventory items.
/// Backed by the <c>menu_view</c> DB view and used as the ES document shape for the "menus" index.
/// </summary>
public class MenuReadModel
{
    public string Id { get; set; } = "";
    public string? Description { get; set; }
    public string OwnerId { get; set; } = "";
    public string? OwnerName { get; set; }
    public string? OwnerImage { get; set; }
    public string? Image { get; set; }
    public bool IsActive { get; set; }
    public string DisplayCurrency { get; set; } = "GHS";

    public string? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? MenuItemCode { get; set; }
    public bool IsPublished { get; set; }
    public bool IsScheduled { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }

    public List<MenuInventoryItem>? InventoryItems { get; set; }
    public string? PriceRange { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class MenuInventoryItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal DisplayPrice { get; set; }
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; }
    public MenuItemVariety? Variety { get; set; }
}

public class MenuItemVariety
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public List<MenuItemVarietyOption> Options { get; set; } = new();
}

public class MenuItemVarietyOption
{
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}
