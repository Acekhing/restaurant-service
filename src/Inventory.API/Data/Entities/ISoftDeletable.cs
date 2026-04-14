namespace Inventory.API.Data.Entities;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}
