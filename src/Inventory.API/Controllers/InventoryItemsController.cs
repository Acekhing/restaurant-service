using Inventory.API.Data;
using Inventory.API.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InventoryItemsController : ControllerBase
{
    private readonly InventoryDbContext _db;

    public InventoryItemsController(InventoryDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InventoryReadModel>>> List(
        [FromQuery] string? itemType,
        [FromQuery] string? retailerId,
        CancellationToken ct)
    {
        var query = _db.InventoryViews.AsNoTracking();
        if (!string.IsNullOrEmpty(itemType))
            query = query.Where(x => x.ItemType == itemType);
        if (!string.IsNullOrEmpty(retailerId))
            query = query.Where(x => x.RetailerId == retailerId);
        var rows = await query.ToListAsync(ct);
        return Ok(rows);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryReadModel>> Get(string id, CancellationToken ct)
    {
        var row = await _db.InventoryViews.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? NotFound() : Ok(row);
    }

    public sealed record CreateItemRequest(
        string Name,
        string RetailerId,
        decimal SupplierPrice,
        decimal DeliveryFee,
        string ItemType,
        int? AveragePreparationTime = null,
        List<Inventory.Contracts.ReadModel.InventoryOpeningHours>? OpeningDayHours = null);

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateItemRequest body, CancellationToken ct)
    {
        var itemId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;

        await EnsureOwnerExistsAsync(body.ItemType, body.RetailerId, ct);

        var displayPrice = body.SupplierPrice * 1.10m;

        var item = new InventoryItem
        {
            Id = itemId,
            Name = body.Name,
            ItemType = body.ItemType,
            RetailerId = body.RetailerId,
            DisplayPrice = displayPrice,
            SupplierPrice = body.SupplierPrice,
            DeliveryFee = body.DeliveryFee,
            DisplayCurrency = "GHS",
            IsDeleted = false,
            HasVariety = false,
            IsOriginalImage = false,
            AveragePreparationTime = body.AveragePreparationTime,
            IsAvailable = true,
            OpeningDayHours = body.OpeningDayHours,
            HasDeals = false,
            InventoryItemCode = GenerateItemCode(),
            CreatedAt = now,
            UpdatedAt = now,
            RowVersion = 0
        };

        _db.InventoryItems.Add(item);

        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await _db.SaveChangesAsync(ct);
                return CreatedAtAction(nameof(Get), new { id = itemId }, new { id = itemId });
            }
            catch (DbUpdateException) when (attempt < 2)
            {
                _db.ChangeTracker.Entries().First(e => e.Entity == item).State = EntityState.Unchanged;
                item.InventoryItemCode = GenerateItemCode();
                _db.Entry(item).State = EntityState.Added;
            }
        }

        return CreatedAtAction(nameof(Get), new { id = itemId }, new { id = itemId });
    }

    private static string GenerateItemCode() =>
        "INV-" + Guid.NewGuid().ToString("N")[..8].ToUpper();

    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
    {
        var item = await _db.InventoryItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.InventoryItemCode == code, ct);
        if (item is null)
            return NotFound();
        return Ok(item);
    }

    public sealed record UpdateInventoryItemCodeRequest(string? InventoryItemCode);

    [HttpPatch("{id}/inventory-item-code")]
    public async Task<IActionResult> UpdateInventoryItemCode(
        string id,
        [FromBody] UpdateInventoryItemCodeRequest body,
        CancellationToken ct)
    {
        var item = await _db.InventoryItems.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null)
            return NotFound();

        if (body.InventoryItemCode is not null)
        {
            var duplicate = await _db.InventoryItems
                .AnyAsync(x => x.InventoryItemCode == body.InventoryItemCode && x.Id != id, ct);
            if (duplicate)
                return Conflict(new { error = "This inventory item code is already in use." });
        }

        item.InventoryItemCode = body.InventoryItemCode;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    public sealed record UpdatePriceRequest(decimal DisplayPrice);

    [HttpPatch("{id}/price")]
    public async Task<IActionResult> UpdatePrice(string id, [FromBody] UpdatePriceRequest body, CancellationToken ct)
    {
        var item = await _db.InventoryItems.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null)
            return NotFound();

        item.WasDisplayPrice = item.DisplayPrice;
        item.DisplayPrice = body.DisplayPrice;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    public sealed record UpdateAvailabilityRequest(bool IsAvailable, bool? OutOfStock, bool? IsHidden);

    [HttpPatch("{id}/availability")]
    public async Task<IActionResult> UpdateAvailability(string id, [FromBody] UpdateAvailabilityRequest body, CancellationToken ct)
    {
        var item = await _db.InventoryItems.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null)
            return NotFound();

        item.IsAvailable = body.IsAvailable;
        if (body.OutOfStock.HasValue)
            item.OutOfStock = body.OutOfStock.Value;
        if (body.IsHidden.HasValue)
            item.IsHidden = body.IsHidden.Value;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("generate-codes")]
    public async Task<ActionResult<object>> GenerateMissingCodes(CancellationToken ct)
    {
        var items = await _db.InventoryItems
            .Where(x => x.InventoryItemCode == null)
            .ToListAsync(ct);

        foreach (var item in items)
        {
            item.InventoryItemCode = "INV-" + Guid.NewGuid().ToString("N")[..8].ToUpper();
            item.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new { generated = items.Count });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var item = await _db.InventoryItems.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null)
            return NotFound();

        _db.InventoryItems.Remove(item);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task EnsureOwnerExistsAsync(string itemType, string ownerId, CancellationToken ct)
    {
        var exists = await _db.Retailers.AsNoTracking().AnyAsync(r => r.Id == ownerId, ct);
        if (!exists)
        {
            _db.Retailers.Add(new Retailer
            {
                Id = ownerId,
                RetailerType = itemType,
                BusinessName = itemType[0..1].ToUpper() + itemType[1..] + " " + ownerId
            });
        }
    }
}
