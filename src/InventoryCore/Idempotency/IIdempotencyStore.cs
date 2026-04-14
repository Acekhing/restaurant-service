namespace InventoryCore.Idempotency;

public interface IIdempotencyStore
{
    /// <summary>
    /// Returns true if this idempotency key was acquired (first time); false if already processed.
    /// </summary>
    Task<bool> TryAcquireAsync(string idempotencyKey, TimeSpan ttl, CancellationToken cancellationToken = default);
}
