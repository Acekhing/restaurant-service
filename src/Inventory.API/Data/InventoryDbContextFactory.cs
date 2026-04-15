using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Inventory.API.Data;

public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var inventoryCs = config.GetConnectionString("Inventory")
            ?? throw new InvalidOperationException(
                "Connection string 'Inventory' is missing. Set ConnectionStrings__Inventory or appsettings.json.");

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseNpgsql(inventoryCs)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new InventoryDbContext(options);
    }
}
