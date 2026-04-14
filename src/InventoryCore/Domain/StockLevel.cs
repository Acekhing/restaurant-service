namespace InventoryCore.Domain;

public readonly record struct StockLevel
{
    public int? Quantity { get; init; }

    public bool IsTracked => Quantity.HasValue;

    public static StockLevel Untracked => new() { Quantity = null };

    public static StockLevel FromQuantity(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        return new StockLevel { Quantity = quantity };
    }
}
