namespace InventoryCore.Zone;

/// <summary>
/// Shared abstraction for station/zone resolution (Redis-backed implementation can be added per deployment).
/// </summary>
public interface IZoneResolver
{
    Task<string?> ResolveZoneIdAsync(string stationId, CancellationToken cancellationToken = default);
}
