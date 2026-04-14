using StackExchange.Redis;

namespace InventoryCore.Idempotency;

public sealed class RedisIdempotencyStore : IIdempotencyStore
{
    private readonly IConnectionMultiplexer _mux;
    private readonly string _prefix;

    public RedisIdempotencyStore(IConnectionMultiplexer mux, string prefix = "outbox:")
    {
        _mux = mux;
        _prefix = prefix;
    }

    public async Task<bool> TryAcquireAsync(string idempotencyKey, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var db = _mux.GetDatabase();
        var key = _prefix + idempotencyKey;
        return await db.StringSetAsync(key, "1", ttl, When.NotExists).ConfigureAwait(false);
    }
}
