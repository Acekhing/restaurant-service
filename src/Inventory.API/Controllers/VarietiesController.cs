using System.Text.Json;
using Inventory.API.Data;
using Inventory.API.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class VarietiesController : ControllerBase
{
    private readonly InventoryDbContext _db;

    public VarietiesController(InventoryDbContext db) => _db = db;

    public sealed record CreateVarietyRequest(
        string Name,
        List<string> InventoryItemIds,
        List<VarietyData> Varieties,
        string OwnerId);

    public sealed record UpdateVarietyRequest(
        string? Name,
        List<string>? InventoryItemIds,
        List<VarietyData>? Varieties);

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateVarietyRequest body, CancellationToken ct)
    {
        var varietyId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;

        var variety = new Variety
        {
            Id = varietyId,
            Name = body.Name,
            InventoryItemIds = JsonSerializer.Serialize(body.InventoryItemIds),
            Varieties = body.Varieties,
            OwnerId = body.OwnerId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Varieties.Add(variety);

        var items = await _db.InventoryItems
            .Where(x => body.InventoryItemIds.Contains(x.Id))
            .ToListAsync(ct);

        foreach (var item in items)
        {
            item.HasVariety = true;
            item.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Get), new { id = varietyId }, new { id = varietyId });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateVarietyRequest body, CancellationToken ct)
    {
        var variety = await _db.Varieties.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (variety is null)
            return NotFound();

        var now = DateTimeOffset.UtcNow;
        var oldItemIds = JsonSerializer.Deserialize<List<string>>(variety.InventoryItemIds) ?? [];

        if (body.Name is not null)
            variety.Name = body.Name;

        if (body.Varieties is not null)
            variety.Varieties = body.Varieties;

        if (body.InventoryItemIds is not null)
        {
            var newIds = body.InventoryItemIds;
            variety.InventoryItemIds = JsonSerializer.Serialize(newIds);

            var removedIds = oldItemIds.Except(newIds).ToList();
            if (removedIds.Count > 0)
            {
                var removedItems = await _db.InventoryItems
                    .Where(x => removedIds.Contains(x.Id))
                    .ToListAsync(ct);
                foreach (var item in removedItems)
                {
                    item.HasVariety = false;
                    item.UpdatedAt = now;
                }
            }

            var addedIds = newIds.Except(oldItemIds).ToList();
            if (addedIds.Count > 0)
            {
                var addedItems = await _db.InventoryItems
                    .Where(x => addedIds.Contains(x.Id))
                    .ToListAsync(ct);
                foreach (var item in addedItems)
                {
                    item.HasVariety = true;
                    item.UpdatedAt = now;
                }
            }
        }

        variety.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken ct)
    {
        var variety = await _db.Varieties.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (variety is null)
            return NotFound();

        var itemIds = JsonSerializer.Deserialize<List<string>>(variety.InventoryItemIds) ?? [];
        var itemNames = await _db.InventoryItems.AsNoTracking()
            .Where(x => itemIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return Ok(new
        {
            variety.Id,
            variety.Name,
            InventoryItemIds = itemIds,
            InventoryItems = itemIds.Select(iid => new { Id = iid, Name = itemNames.GetValueOrDefault(iid, iid) }),
            Varieties = variety.Varieties,
            variety.OwnerId,
            variety.CreatedAt,
            variety.UpdatedAt
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var variety = await _db.Varieties.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (variety is null)
            return NotFound();

        var now = DateTimeOffset.UtcNow;
        var itemIds = JsonSerializer.Deserialize<List<string>>(variety.InventoryItemIds) ?? [];

        if (itemIds.Count > 0)
        {
            var items = await _db.InventoryItems
                .Where(x => itemIds.Contains(x.Id))
                .ToListAsync(ct);

            foreach (var item in items)
            {
                item.HasVariety = false;
                item.UpdatedAt = now;
            }
        }

        _db.Varieties.Remove(variety);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? ownerId,
        CancellationToken ct)
    {
        var query = _db.Varieties.AsNoTracking();
        if (!string.IsNullOrEmpty(ownerId))
            query = query.Where(x => x.OwnerId == ownerId);

        var rows = await query.OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

        var allItemIds = rows.SelectMany(v =>
            JsonSerializer.Deserialize<List<string>>(v.InventoryItemIds) ?? []).Distinct().ToList();
        var itemNames = await _db.InventoryItems.AsNoTracking()
            .Where(x => allItemIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var result = rows.Select(v =>
        {
            var ids = JsonSerializer.Deserialize<List<string>>(v.InventoryItemIds) ?? [];
            return new
            {
                v.Id,
                v.Name,
                InventoryItemIds = ids,
                InventoryItems = ids.Select(iid => new { Id = iid, Name = itemNames.GetValueOrDefault(iid, iid) }),
                Varieties = v.Varieties,
                v.OwnerId,
                v.CreatedAt,
                v.UpdatedAt
            };
        });

        return Ok(result);
    }
}
