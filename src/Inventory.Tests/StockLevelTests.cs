using InventoryCore.Domain;

namespace Inventory.Tests;

public sealed class StockLevelTests
{
    [Fact]
    public void FromQuantity_throws_when_negative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StockLevel.FromQuantity(-1));
    }

    [Fact]
    public void FromQuantity_returns_tracked_quantity()
    {
        var s = StockLevel.FromQuantity(5);
        Assert.True(s.IsTracked);
        Assert.Equal(5, s.Quantity);
    }
}
