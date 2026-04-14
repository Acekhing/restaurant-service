namespace Inventory.API.Data.Entities;

public sealed class MenuHistory
{
    public Guid Id { get; set; }
    public string MenuId { get; set; } = "";
    public string OwnerId { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTimeOffset Date { get; set; }
}
