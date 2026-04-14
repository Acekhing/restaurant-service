namespace Inventory.API.Data.Entities;

/// <summary>
/// EF-mapped wrapper over the shared <see cref="Inventory.Contracts.ReadModel.BranchReadModel"/>
/// that binds to the <c>branch_view</c> database view.
/// </summary>
public sealed class BranchReadModel : Inventory.Contracts.ReadModel.BranchReadModel;
