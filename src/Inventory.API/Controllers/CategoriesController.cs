using Inventory.API.Data;
using Inventory.API.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CategoriesController : ControllerBase
{
    private readonly InventoryDbContext _db;

    public CategoriesController(InventoryDbContext db) => _db = db;

    public sealed record CreateCategoryRequest(string Name, string OwnerId);
    public sealed record UpdateCategoryRequest(string Name);

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateCategoryRequest body, CancellationToken ct)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = body.Name,
            OwnerId = body.OwnerId
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);

        return Ok(new { id = category.Id });
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? ownerId, CancellationToken ct)
    {
        var query = _db.Categories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(ownerId))
            query = query.Where(x => x.OwnerId == ownerId);

        var categories = await query
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.OwnerId })
            .ToListAsync(ct);

        return Ok(categories);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest body, CancellationToken ct)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (category is null)
            return NotFound();

        category.Name = body.Name;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (category is null)
            return NotFound();

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
