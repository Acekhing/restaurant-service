namespace InventoryCore.Zone;

public sealed class NoOpZoneResolver : IZoneResolver
{
    public Task<string?> ResolveZoneIdAsync(string stationId, CancellationToken cancellationToken = default)
        => Task.FromResult<string?>(null);
}
