using Inventory.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/audit-log")]
public sealed class AuditLogController : ControllerBase
{
    private readonly InventoryDbContext _db;

    public AuditLogController(InventoryDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? aggregateId,
        [FromQuery] string? eventType,
        [FromQuery] string? aggregateType,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        size = Math.Clamp(size, 1, 100);
        page = Math.Max(1, page);

        var query = _db.AuditLog.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(aggregateId))
            query = query.Where(x => x.AggregateId == aggregateId);
        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(x => x.EventType == eventType);
        if (!string.IsNullOrWhiteSpace(aggregateType))
            query = query.Where(x => x.AggregateType == aggregateType);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.OccurredAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return Ok(new { items, total, page, size });
    }

    [HttpGet("{aggregateId}")]
    public async Task<IActionResult> GetByAggregateId(string aggregateId, CancellationToken ct)
    {
        var entries = await _db.AuditLog.AsNoTracking()
            .Where(x => x.AggregateId == aggregateId)
            .OrderByDescending(x => x.OccurredAt)
            .ToListAsync(ct);

        return Ok(entries);
    }
}
