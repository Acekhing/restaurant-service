using InventoryCore.Idempotency;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace Inventory.Tests;

[Trait("Category", "DockerIntegration")]
public sealed class RedisIdempotencyTests : IAsyncLifetime
{
    private RedisContainer? _redis;

    public async Task InitializeAsync()
    {
        _redis = new RedisBuilder().Build();
        await _redis.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_redis is not null)
            await _redis.DisposeAsync();
    }

    [Fact]
    public async Task TryAcquire_same_key_second_call_returns_false()
    {
        var mux = await ConnectionMultiplexer.ConnectAsync(_redis!.GetConnectionString());
        var store = new RedisIdempotencyStore(mux);
        var key = Guid.NewGuid().ToString();

        var first = await store.TryAcquireAsync(key, TimeSpan.FromMinutes(5));
        var second = await store.TryAcquireAsync(key, TimeSpan.FromMinutes(5));

        Assert.True(first);
        Assert.False(second);
    }
}
