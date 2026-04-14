namespace Inventory.API.Data.Entities;

/// <summary>
/// EF-mapped wrapper over the shared <see cref="Inventory.Contracts.ReadModel.MenuReadModel"/>
/// that binds to the <c>menu_view</c> database view.
/// Adds <see cref="ItemsJson"/> for internal view-to-model deserialization.
/// </summary>
public sealed class MenuReadModel : Inventory.Contracts.ReadModel.MenuReadModel
{
    public string? ItemsJson { get; set; }
}
