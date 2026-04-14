using Inventory.API.Data.Entities;
using InventoryCore.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Inventory.API.Services;

public sealed class InventoryOutboxEntityMapper : IOutboxEntityMapper
{
    public OutboxMessageDescriptor? Describe(EntityEntry entry)
    {
        switch (entry.Entity)
        {
            case InventoryItem:
                {
                    var id = GetStringId(entry, nameof(InventoryItem.Id));
                    return entry.State switch
                    {
                        EntityState.Added => new OutboxMessageDescriptor(id, nameof(InventoryItem), "ItemCreated"),
                        EntityState.Modified => new OutboxMessageDescriptor(id, nameof(InventoryItem), "ItemUpdated"),
                        EntityState.Deleted => new OutboxMessageDescriptor(id, nameof(InventoryItem), "ItemDeleted"),
                        _ => null
                    };
                }
            case InventoryItemPromotion promo:
                {
                    var eventType = entry.State switch
                    {
                        EntityState.Added => "PromotionCreated",
                        EntityState.Modified => "PromotionUpdated",
                        EntityState.Deleted => "PromotionDeleted",
                        _ => (string?)null
                    };
                    return eventType is null
                        ? null
                        : new OutboxMessageDescriptor(
                            promo.OwnerId,
                            nameof(InventoryItemPromotion),
                            eventType);
                }
            case Menu:
                {
                    var id = GetStringId(entry, nameof(Menu.Id));
                    return entry.State switch
                    {
                        EntityState.Added => new OutboxMessageDescriptor(id, nameof(Menu), "MenuCreated"),
                        EntityState.Modified => new OutboxMessageDescriptor(id, nameof(Menu), "MenuUpdated"),
                        EntityState.Deleted => new OutboxMessageDescriptor(id, nameof(Menu), "MenuDeleted"),
                        _ => null
                    };
                }
            case Variety:
                {
                    var id = GetStringId(entry, nameof(Variety.Id));
                    return entry.State switch
                    {
                        EntityState.Added => new OutboxMessageDescriptor(id, nameof(Variety), "VarietyCreated"),
                        EntityState.Modified => new OutboxMessageDescriptor(id, nameof(Variety), "VarietyUpdated"),
                        EntityState.Deleted => new OutboxMessageDescriptor(id, nameof(Variety), "VarietyDeleted"),
                        _ => null
                    };
                }
            case Branch:
                {
                    var id = GetStringId(entry, nameof(Branch.Id));
                    return entry.State switch
                    {
                        EntityState.Added => new OutboxMessageDescriptor(id, nameof(Branch), "BranchCreated"),
                        EntityState.Modified => new OutboxMessageDescriptor(id, nameof(Branch), "BranchUpdated"),
                        EntityState.Deleted => new OutboxMessageDescriptor(id, nameof(Branch), "BranchDeleted"),
                        _ => null
                    };
                }
            case InventoryOutbox:
                return null;
            default:
                return null;
        }
    }

    private static string GetStringId(EntityEntry entry, string propertyName) =>
        entry.State == EntityState.Deleted
            ? (string)entry.Property(propertyName).OriginalValue!
            : (string)entry.Property(propertyName).CurrentValue!;
}
