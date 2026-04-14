using Inventory.API.Data;
using Inventory.API.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly InventoryDbContext _db;

    public UsersController(InventoryDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> List(
        [FromQuery] string? retailerId,
        CancellationToken ct)
    {
        var query = _db.Users.AsNoTracking().Where(u => u.IsActive || true);
        if (!string.IsNullOrEmpty(retailerId))
            query = query.Where(u => u.RetailerId == retailerId);

        var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync(ct);

        var retailerIds = users.Select(u => u.RetailerId).Distinct().ToList();
        var retailers = await _db.Retailers
            .AsNoTracking()
            .Where(r => retailerIds.Contains(r.Id))
            .Select(r => new { r.Id, r.BusinessName })
            .ToDictionaryAsync(r => r.Id, r => r.BusinessName, ct);

        var dtos = users.Select(u => ToDto(u, retailers.GetValueOrDefault(u.RetailerId))).ToList();
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> Get(string id, CancellationToken ct)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return NotFound();

        var retailerName = await _db.Retailers
            .AsNoTracking()
            .Where(r => r.Id == user.RetailerId)
            .Select(r => r.BusinessName)
            .FirstOrDefaultAsync(ct);

        return Ok(ToDto(user, retailerName));
    }

    public sealed record CreateUserRequest(
        string Email,
        string Password,
        string? FullName,
        string? Role,
        string RetailerId);

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateUserRequest body, CancellationToken ct)
    {
        var emailLower = body.Email.Trim().ToLowerInvariant();

        var duplicate = await _db.Users.AnyAsync(u => u.Email == emailLower, ct);
        if (duplicate)
            return Conflict(new { error = "A user with this email already exists." });

        var retailer = await _db.Retailers.AsNoTracking().FirstOrDefaultAsync(r => r.Id == body.RetailerId, ct);
        if (retailer is null)
            return BadRequest(new { error = "Retailer not found." });

        var user = new User
        {
            Email = emailLower,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(body.Password),
            FullName = body.FullName,
            Role = body.Role,
            RetailerId = body.RetailerId,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Get), new { id = user.Id }, new { id = user.Id });
    }

    public sealed record UpdateUserRequest(
        string? FullName,
        string? Role,
        string? RetailerId,
        bool? IsActive);

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest body, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return NotFound();

        if (body.FullName is not null) user.FullName = body.FullName;
        if (body.Role is not null) user.Role = body.Role;
        if (body.RetailerId is not null)
        {
            var retailer = await _db.Retailers.AsNoTracking().AnyAsync(r => r.Id == body.RetailerId, ct);
            if (!retailer)
                return BadRequest(new { error = "Retailer not found." });
            user.RetailerId = body.RetailerId;
        }
        if (body.IsActive.HasValue) user.IsActive = body.IsActive.Value;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    public sealed record ChangePasswordRequest(string Password);

    [HttpPatch("{id}/password")]
    public async Task<IActionResult> ChangePassword(
        string id,
        [FromBody] ChangePasswordRequest body,
        CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return NotFound();

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(body.Password);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    public sealed record LoginRequest(string Email, string Password);

    public sealed record LoginResponse(
        string Id,
        string Email,
        string? FullName,
        string? Role,
        string RetailerId,
        string? RetailerName);

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest body, CancellationToken ct)
    {
        var emailLower = body.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == emailLower, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(body.Password, user.PasswordHash))
            return Unauthorized(new { error = "Invalid email or password." });

        if (!user.IsActive)
            return Unauthorized(new { error = "Account is disabled. Contact your manager." });

        var retailerName = await _db.Retailers
            .AsNoTracking()
            .Where(r => r.Id == user.RetailerId)
            .Select(r => r.BusinessName)
            .FirstOrDefaultAsync(ct);

        return Ok(new LoginResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.Role,
            user.RetailerId,
            retailerName));
    }

    private static UserDto ToDto(User u, string? retailerName) => new(
        u.Id,
        u.Email,
        u.FullName,
        u.Role,
        u.RetailerId,
        retailerName,
        u.IsActive,
        u.CreatedAt);

    public sealed record UserDto(
        string Id,
        string Email,
        string? FullName,
        string? Role,
        string RetailerId,
        string? RetailerName,
        bool IsActive,
        DateTime CreatedAt);
}
