using System.Text.Json;
using Inventory.API.Data;
using Inventory.API.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TimetablesController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly InventoryDbContext _db;

    public TimetablesController(InventoryDbContext db) => _db = db;

    public sealed record CreateTimetableRequest(
        string Name,
        string? Description,
        string OwnerId,
        List<TimetableOpening> Openings);

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateTimetableRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.Name))
            return BadRequest(new { error = "Name is required." });

        if (body.Openings is not { Count: > 0 })
            return BadRequest(new { error = "At least one opening is required." });

        var id = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;

        var timetable = new Timetable
        {
            Id = id,
            Name = body.Name,
            Description = body.Description,
            OwnerId = body.OwnerId,
            Openings = JsonSerializer.Serialize(body.Openings, JsonOpts),
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Timetables.Add(timetable);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken ct)
    {
        var timetable = await _db.Timetables.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (timetable is null) return NotFound();
        return Ok(ToResponse(timetable));
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? ownerId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(ownerId))
            return BadRequest(new { error = "ownerId query parameter is required." });

        var rows = await _db.Timetables
            .AsNoTracking()
            .Where(x => x.OwnerId == ownerId)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        return Ok(rows.Select(ToResponse));
    }

    public sealed record UpdateTimetableRequest(
        string Name,
        string? Description,
        List<TimetableOpening> Openings);

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateTimetableRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.Name))
            return BadRequest(new { error = "Name is required." });

        if (body.Openings is not { Count: > 0 })
            return BadRequest(new { error = "At least one opening is required." });

        var timetable = await _db.Timetables.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (timetable is null)
            return NotFound();

        timetable.Name = body.Name;
        timetable.Description = body.Description;
        timetable.Openings = JsonSerializer.Serialize(body.Openings, JsonOpts);
        timetable.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var timetable = await _db.Timetables.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (timetable is null)
            return NotFound();

        _db.Timetables.Remove(timetable);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static object ToResponse(Timetable t) => new
    {
        t.Id,
        t.Name,
        t.Description,
        t.OwnerId,
        Openings = JsonSerializer.Deserialize<List<TimetableOpening>>(t.Openings, JsonOpts) ?? [],
        t.CreatedAt,
        t.UpdatedAt
    };
}
