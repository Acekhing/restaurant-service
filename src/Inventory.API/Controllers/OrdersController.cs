using Inventory.API.Data;
using Inventory.API.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly InventoryDbContext _db;

    public OrdersController(InventoryDbContext db) => _db = db;

    public sealed record CreateOrderLineRequest(
        string InventoryItemId,
        string ItemName,
        decimal UnitPrice,
        int Quantity,
        string? Notes,
        string? VarietySelection);

    public sealed record CreateOrderRequest(
        string RetailerId,
        string? WaiterName,
        string? TableNumber,
        string? CustomerNotes,
        string? CustomerPhone,
        string? DisplayCurrency,
        List<CreateOrderLineRequest> Lines);

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateOrderRequest body, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var todayStart = now.Date;
        var todayEnd = todayStart.AddDays(1);

        var todayCount = await _db.Orders
            .CountAsync(o => o.RetailerId == body.RetailerId
                && o.CreatedAt >= new DateTimeOffset(todayStart, TimeSpan.Zero)
                && o.CreatedAt < new DateTimeOffset(todayEnd, TimeSpan.Zero), ct);

        var orderNumber = (todayCount + 1).ToString("D3");

        var totalAmount = body.Lines.Sum(l => l.UnitPrice * l.Quantity);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            RetailerId = body.RetailerId,
            WaiterName = body.WaiterName,
            TableNumber = body.TableNumber,
            CustomerNotes = body.CustomerNotes,
            CustomerPhone = body.CustomerPhone,
            Status = "Pending",
            TotalAmount = totalAmount,
            DisplayCurrency = body.DisplayCurrency ?? "GHS",
            CreatedAt = now,
            UpdatedAt = now,
            Lines = body.Lines.Select(l => new OrderLine
            {
                Id = Guid.NewGuid(),
                InventoryItemId = l.InventoryItemId,
                ItemName = l.ItemName,
                UnitPrice = l.UnitPrice,
                Quantity = l.Quantity,
                Notes = l.Notes,
                VarietySelection = l.VarietySelection,
            }).ToList()
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Get), new { id = order.Id }, new
        {
            id = order.Id,
            orderNumber = order.OrderNumber,
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order is null)
            return NotFound();

        return Ok(MapToDto(order));
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? retailerId,
        [FromQuery] string? status,
        [FromQuery] DateTime? date,
        CancellationToken ct)
    {
        var query = _db.Orders.AsNoTracking().Include(o => o.Lines).AsQueryable();

        if (!string.IsNullOrEmpty(retailerId))
            query = query.Where(o => o.RetailerId == retailerId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status == status);

        if (date.HasValue)
        {
            var dayStart = new DateTimeOffset(date.Value.Date, TimeSpan.Zero);
            var dayEnd = dayStart.AddDays(1);
            query = query.Where(o => o.CreatedAt >= dayStart && o.CreatedAt < dayEnd);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        return Ok(orders.Select(MapToDto));
    }

    public sealed record UpdateStatusRequest(string Status);

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest body, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
            return NotFound();

        string[] validStatuses = ["Pending", "Confirmed", "Preparing", "Ready", "Served", "Cancelled"];
        if (!validStatuses.Contains(body.Status))
            return BadRequest(new { error = $"Invalid status. Must be one of: {string.Join(", ", validStatuses)}" });

        order.Status = body.Status;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    private static object MapToDto(Order order) => new
    {
        order.Id,
        order.OrderNumber,
        order.RetailerId,
        order.WaiterName,
        order.TableNumber,
        order.CustomerNotes,
        order.CustomerPhone,
        order.Status,
        order.TotalAmount,
        order.DisplayCurrency,
        order.CreatedAt,
        order.UpdatedAt,
        Lines = order.Lines.Select(l => new
        {
            l.Id,
            l.OrderId,
            l.InventoryItemId,
            l.ItemName,
            l.UnitPrice,
            l.Quantity,
            l.Notes,
            l.VarietySelection,
        }),
    };
}
