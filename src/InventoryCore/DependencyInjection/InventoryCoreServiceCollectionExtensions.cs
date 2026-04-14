using InventoryCore.Idempotency;
using InventoryCore.Zone;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace InventoryCore.DependencyInjection;

public static class InventoryCoreServiceCollectionExtensions
{
    public static IServiceCollection AddRedisIdempotencyStore(
        this IServiceCollection services,
        string redisConnectionString,
        string keyPrefix = "outbox:")
    {
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddSingleton<IIdempotencyStore>(sp =>
            new RedisIdempotencyStore(sp.GetRequiredService<IConnectionMultiplexer>(), keyPrefix));
        return services;
    }

    public static IServiceCollection AddNoOpZoneResolver(this IServiceCollection services)
    {
        services.AddSingleton<IZoneResolver, NoOpZoneResolver>();
        return services;
    }
}
