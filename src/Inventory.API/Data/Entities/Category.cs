namespace Inventory.API.Data.Entities;

public sealed class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string OwnerId { get; set; } = "";
}
