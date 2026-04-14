using Inventory.Contracts.ReadModel;

namespace Inventory.API.Data.Entities;

/// <summary>
/// EF-mapped wrapper over the shared <see cref="Inventory.Contracts.ReadModel.InventoryReadModel"/>
/// that binds to the <c>inventory_view</c> database view.
/// Inherits all properties from the shared contract.
/// </summary>
public sealed class InventoryReadModel : Inventory.Contracts.ReadModel.InventoryReadModel;
