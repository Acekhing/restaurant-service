namespace Inventory.API.Data.Entities;

public sealed class Timetable
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string OwnerId { get; set; } = "";
    public string Openings { get; set; } = "[]";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class TimetableOpening
{
    public string Name { get; set; } = "";
    public string OpeningTime { get; set; } = "";
    public string ClosingTime { get; set; } = "";
}
