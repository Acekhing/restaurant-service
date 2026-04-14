namespace Inventory.API.Data.Entities;

/// <summary>Minimal owner row for read-model view joins.</summary>
public sealed class Pharmacy
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Image { get; set; }
}
