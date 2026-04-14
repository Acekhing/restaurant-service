using System.Text.Json;
using Inventory.API.Data;
using Inventory.API.Data.Entities;
using Inventory.Contracts.ReadModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MenusController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };

    private readonly InventoryDbContext _db;

    public MenusController(InventoryDbContext db) => _db = db;

    public sealed record CreateMenuRequest(
        string? Description,
        string OwnerId,
        string? Image,
        string? DisplayCurrency,
        Guid CategoryId,
        bool? IsPublished,
        bool? IsScheduled,
        DateTimeOffset? PublishedAt,
        List<MenuInventory>? InventoryItems);

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateMenuRequest body, CancellationToken ct)
    {
        var menuId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;

        var menu = new Menu
        {
            Id = menuId,
            Description = body.Description,
            OwnerId = body.OwnerId,
            Image = body.Image,
            DisplayCurrency = body.DisplayCurrency ?? "GHS",
            IsActive = true,
            CategoryId = body.CategoryId,
            IsPublished = body.IsPublished ?? false,
            IsScheduled = body.IsScheduled ?? false,
            PublishedAt = body.PublishedAt,
            InventoryItemIds = body.InventoryItems,
            MenuItemCode = GenerateMenuCode(),
            CreatedAt = now,
            UpdatedAt = now,
            RowVersion = 0
        };

        _db.Menus.Add(menu);
        _db.MenuHistories.Add(new MenuHistory
        {
            Id = Guid.NewGuid(),
            MenuId = menuId,
            OwnerId = body.OwnerId,
            Status = "Created",
            Date = now,
        });

        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await _db.SaveChangesAsync(ct);
                return CreatedAtAction(nameof(Get), new { id = menuId }, new { id = menuId });
            }
            catch (DbUpdateException) when (attempt < 2)
            {
                _db.ChangeTracker.Entries()
                    .Where(e => e.Entity == menu || e.Entity is MenuHistory)
                    .ToList()
                    .ForEach(e => e.State = EntityState.Unchanged);
                menu.MenuItemCode = GenerateMenuCode();
                _db.Entry(menu).State = EntityState.Added;
                _db.ChangeTracker.Entries()
                    .Where(e => e.Entity is MenuHistory h && h.MenuId == menuId)
                    .ToList()
                    .ForEach(e => e.State = EntityState.Added);
            }
        }

        return CreatedAtAction(nameof(Get), new { id = menuId }, new { id = menuId });
    }

    private static string GenerateMenuCode() =>
        "MNU-" + Guid.NewGuid().ToString("N")[..8].ToUpper();

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken ct)
    {
        var row = await _db.MenuViews.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (row is null) return NotFound();
        EnrichInventoryItems(row);
        return Ok(row);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? ownerId,
        CancellationToken ct)
    {
        var query = _db.MenuViews.AsNoTracking();
        if (!string.IsNullOrEmpty(ownerId))
            query = query.Where(x => x.OwnerId == ownerId);
        var rows = await query.ToListAsync(ct);
        foreach (var row in rows)
            EnrichInventoryItems(row);
        return Ok(rows);
    }

    public sealed record UpdateMenuRequest(
        string? Description,
        string? Image,
        bool? IsActive,
        Guid? CategoryId,
        bool? IsPublished,
        bool? IsScheduled,
        DateTimeOffset? PublishedAt,
        List<MenuInventory>? InventoryItems);

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateMenuRequest body, CancellationToken ct)
    {
        var menu = await _db.Menus.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (menu is null)
            return NotFound();

        var wasPublished = menu.IsPublished;
        var wasScheduled = menu.IsScheduled;

        if (body.Description is not null) menu.Description = body.Description;
        if (body.Image is not null) menu.Image = body.Image;
        if (body.IsActive.HasValue) menu.IsActive = body.IsActive.Value;
        if (body.CategoryId.HasValue) menu.CategoryId = body.CategoryId.Value;
        if (body.IsPublished.HasValue) menu.IsPublished = body.IsPublished.Value;
        if (body.IsScheduled.HasValue) menu.IsScheduled = body.IsScheduled.Value;
        if (body.PublishedAt.HasValue) menu.PublishedAt = body.PublishedAt.Value;
        if (body.InventoryItems is not null) menu.InventoryItemIds = body.InventoryItems;
        menu.UpdatedAt = DateTimeOffset.UtcNow;

        var status = !wasPublished && menu.IsPublished ? "Published"
            : !wasScheduled && menu.IsScheduled ? "Scheduled"
            : "Edited";

        _db.MenuHistories.Add(new MenuHistory
        {
            Id = Guid.NewGuid(),
            MenuId = id,
            OwnerId = menu.OwnerId,
            Status = status,
            Date = DateTimeOffset.UtcNow,
        });

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
    {
        var menu = await _db.Menus.AsNoTracking().FirstOrDefaultAsync(x => x.MenuItemCode == code, ct);
        if (menu is null)
            return NotFound();

        var view = await _db.MenuViews.AsNoTracking().FirstOrDefaultAsync(x => x.Id == menu.Id, ct);
        if (view is null)
            return NotFound();
        EnrichInventoryItems(view);
        return Ok(view);
    }

    public sealed record UpdateMenuItemCodeRequest(string? MenuItemCode);

    [HttpPatch("{id}/menu-item-code")]
    public async Task<IActionResult> UpdateMenuItemCode(
        string id,
        [FromBody] UpdateMenuItemCodeRequest body,
        CancellationToken ct)
    {
        var menu = await _db.Menus.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (menu is null)
            return NotFound();

        if (body.MenuItemCode is not null)
        {
            var duplicate = await _db.Menus
                .AnyAsync(x => x.MenuItemCode == body.MenuItemCode && x.Id != id, ct);
            if (duplicate)
                return Conflict(new { error = "This menu item code is already in use." });
        }

        menu.MenuItemCode = body.MenuItemCode;
        menu.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("generate-codes")]
    public async Task<ActionResult<object>> GenerateMissingCodes(CancellationToken ct)
    {
        var menus = await _db.Menus
            .Where(x => x.MenuItemCode == null)
            .ToListAsync(ct);

        foreach (var menu in menus)
        {
            menu.MenuItemCode = "MNU-" + Guid.NewGuid().ToString("N")[..8].ToUpper();
            menu.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new { generated = menus.Count });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var menu = await _db.Menus.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (menu is null)
            return NotFound();

        _db.Menus.Remove(menu);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(string id, CancellationToken ct)
    {
        var history = await _db.MenuHistories
            .AsNoTracking()
            .Where(x => x.MenuId == id)
            .OrderByDescending(x => x.Date)
            .ToListAsync(ct);
        return Ok(history);
    }

    public sealed record AddMenuItemsRequest(List<MenuInventory> Items);

    [HttpPost("{id}/items")]
    public async Task<IActionResult> AddItems(string id, [FromBody] AddMenuItemsRequest body, CancellationToken ct)
    {
        var menu = await _db.Menus.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (menu is null)
            return NotFound();

        var existing = menu.InventoryItemIds ?? [];
        var existingIds = existing.Select(e => e.InventoryItemId).ToHashSet();
        foreach (var item in body.Items)
        {
            if (!existingIds.Contains(item.InventoryItemId))
                existing.Add(item);
        }

        menu.InventoryItemIds = existing;
        menu.UpdatedAt = DateTimeOffset.UtcNow;
        _db.MenuHistories.Add(new MenuHistory
        {
            Id = Guid.NewGuid(),
            MenuId = id,
            OwnerId = menu.OwnerId,
            Status = "Edited",
            Date = DateTimeOffset.UtcNow,
        });
        await _db.SaveChangesAsync(ct);

        return Ok(new { added = body.Items.Count });
    }

    [HttpDelete("{id}/items/{inventoryItemId}")]
    public async Task<IActionResult> RemoveItem(string id, string inventoryItemId, CancellationToken ct)
    {
        var menu = await _db.Menus.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (menu is null)
            return NotFound();

        var existing = menu.InventoryItemIds ?? [];
        var removed = existing.RemoveAll(e => e.InventoryItemId == inventoryItemId);
        if (removed == 0)
            return NotFound();

        menu.InventoryItemIds = existing.Count > 0 ? existing : null;
        menu.UpdatedAt = DateTimeOffset.UtcNow;
        _db.MenuHistories.Add(new MenuHistory
        {
            Id = Guid.NewGuid(),
            MenuId = id,
            OwnerId = menu.OwnerId,
            Status = "Edited",
            Date = DateTimeOffset.UtcNow,
        });
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static void EnrichInventoryItems(Data.Entities.MenuReadModel row)
    {
        if (row.ItemsJson is not null)
        {
            row.InventoryItems = JsonSerializer.Deserialize<List<MenuInventoryItem>>(row.ItemsJson, JsonOpts);
            row.ItemsJson = null;
        }
    }
}
