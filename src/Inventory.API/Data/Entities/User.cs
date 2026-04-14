namespace Inventory.API.Data.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public string RetailerId { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
