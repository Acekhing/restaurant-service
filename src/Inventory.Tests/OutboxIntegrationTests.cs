using System.Net.Http.Json;
using Npgsql;

namespace Inventory.Tests;

/// <summary>Requires Docker (Testcontainers). Run: <c>dotnet test --filter DockerIntegration</c></summary>
[Trait("Category", "DockerIntegration")]
public sealed class OutboxIntegrationTests : IClassFixture<InventoryApiFactory>
{
    private readonly InventoryApiFactory _factory;

    public OutboxIntegrationTests(InventoryApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Create_item_persists_outbox_events()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/InventoryItems",
            new
            {
                name = "Integration Test Item",
                ownerId = "owner-1",
                displayPrice = 12.50m,
                itemType = "restaurant"
            });

        response.EnsureSuccessStatusCode();

        await using var conn = new NpgsqlConnection(_factory.ConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM inventory_outbox;", conn);
        var count = (long)(await cmd.ExecuteScalarAsync())!;
        Assert.True(count >= 2, "Expected outbox rows for item and owner.");
    }
}
