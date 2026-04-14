using Inventory.API.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Inventory.Tests;

public sealed class InventoryApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder().Build();

    public string ConnectionString { get; private set; } = "";

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        var adminCs = _postgres.GetConnectionString();
        await using (var conn = new NpgsqlConnection(adminCs))
        {
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("CREATE DATABASE inventory;", conn);
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == "42P04")
            {
            }
        }

        var csb = new NpgsqlConnectionStringBuilder(adminCs) { Database = "inventory" };
        ConnectionString = csb.ToString();

        Environment.SetEnvironmentVariable("ConnectionStrings__Inventory", ConnectionString);

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
        await using var ctx = new InventoryDbContext(options);
        await ctx.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
