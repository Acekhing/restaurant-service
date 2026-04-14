using System.Text.Json;
using Inventory.API.Data;
using Inventory.API.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PromotionsController : ControllerBase
{
    private readonly InventoryDbContext _db;

    public PromotionsController(InventoryDbContext db) => _db = db;

    public sealed record CreatePromotionRequest(
        string OwnerId,
        int DiscountInPercentage,
        string? Currency,
        DateTimeOffset EffectiveFrom,
        DateTimeOffset EffectiveTo,
        string[]? InventoryItemIds,
        string[]? MenuIds,
        bool IsAppliedToMenu,
        bool IsAppliedToItems,
        bool IsFreeDelivery);

    public sealed record UpdatePromotionRequest(
        int DiscountInPercentage,
        string? Currency,
        DateTimeOffset EffectiveFrom,
        DateTimeOffset EffectiveTo,
        string[]? InventoryItemIds,
        string[]? MenuIds,
        bool IsAppliedToMenu,
        bool IsAppliedToItems,
        bool IsFreeDelivery);

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreatePromotionRequest body, CancellationToken ct)
    {
        var promotionId = Guid.NewGuid().ToString();
        var currency = body.Currency ?? "GHS";
        var now = DateTimeOffset.UtcNow;

        var promotion = new InventoryItemPromotion
        {
            Id = promotionId,
            OwnerId = body.OwnerId,
            DiscountInPercentage = body.DiscountInPercentage,
            Currency = currency,
            EffectiveFrom = body.EffectiveFrom,
            EffectiveTo = body.EffectiveTo,
            IsActive = true,
            InventoryItemIds = SerializeIds(body.InventoryItemIds),
            MenuIds = SerializeIds(body.MenuIds),
            IsAppliedToMenu = body.IsAppliedToMenu,
            IsAppliedToItems = body.IsAppliedToItems,
            IsFreeDelivery = body.IsFreeDelivery,
            CreatedAt = now
        };

        _db.InventoryPromotions.Add(promotion);
        await _db.SaveChangesAsync(ct);

        return Ok(new { promotionId });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePromotionRequest body, CancellationToken ct)
    {
        var promotion = await _db.InventoryPromotions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (promotion is null)
            return NotFound();

        promotion.DiscountInPercentage = body.DiscountInPercentage;
        promotion.Currency = body.Currency ?? promotion.Currency;
        promotion.EffectiveFrom = body.EffectiveFrom;
        promotion.EffectiveTo = body.EffectiveTo;
        promotion.InventoryItemIds = SerializeIds(body.InventoryItemIds);
        promotion.MenuIds = SerializeIds(body.MenuIds);
        promotion.IsAppliedToMenu = body.IsAppliedToMenu;
        promotion.IsAppliedToItems = body.IsAppliedToItems;
        promotion.IsFreeDelivery = body.IsFreeDelivery;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var promotion = await _db.InventoryPromotions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (promotion is null)
            return NotFound();

        _db.InventoryPromotions.Remove(promotion);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(string id, CancellationToken ct)
    {
        var promotion = await _db.InventoryPromotions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (promotion is null)
            return NotFound();

        promotion.IsActive = false;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? ownerId, CancellationToken ct)
    {
        var query = _db.InventoryPromotions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(ownerId))
            query = query.Where(x => x.OwnerId == ownerId);

        var promotions = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        var result = promotions.Select(x => new
        {
            x.Id,
            x.OwnerId,
            x.DiscountInPercentage,
            x.Currency,
            x.EffectiveFrom,
            x.EffectiveTo,
            x.IsActive,
            InventoryItemIds = DeserializeIds(x.InventoryItemIds),
            MenuIds = DeserializeIds(x.MenuIds),
            x.IsAppliedToMenu,
            x.IsAppliedToItems,
            x.IsFreeDelivery,
            x.CreatedAt
        });

        return Ok(result);
    }

    private static string? SerializeIds(string[]? ids) =>
        ids is { Length: > 0 }
            ? JsonSerializer.Serialize(ids)
            : null;

    private static string[]? DeserializeIds(string? json) =>
        string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<string[]>(json);
}
