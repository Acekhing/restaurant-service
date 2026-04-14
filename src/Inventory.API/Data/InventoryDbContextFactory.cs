using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Inventory.API.Data;

public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseNpgsql(config.GetConnectionString("Inventory"))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new InventoryDbContext(options);
    }
}
