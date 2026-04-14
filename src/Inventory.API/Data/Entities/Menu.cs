using InventoryCore.RowVersion;

namespace Inventory.API.Data.Entities;

public sealed class Menu : IRowVersioned
{
    public string Id { get; set; } = "";
    public string? Description { get; set; }
    public string OwnerId { get; set; } = "";
    public string? Image { get; set; }
    public string DisplayCurrency { get; set; } = "GHS";
    public bool IsActive { get; set; } = true;

    public Guid? CategoryId { get; set; }
    public string? MenuItemCode { get; set; }
    public bool IsPublished { get; set; }
    public bool IsScheduled { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public List<MenuInventory>? InventoryItemIds { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public long RowVersion { get; set; }
}

public class MenuInventory
{
    public string InventoryItemId { get; set; } = "";
    public bool IsRequired { get; set; }
}
