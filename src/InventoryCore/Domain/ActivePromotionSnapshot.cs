namespace InventoryCore.Domain;

public sealed class ActivePromotionSnapshot
{
    public string PromotionId { get; set; } = "";
    public int DiscountInPercentage { get; set; }
    public string Currency { get; set; } = "GHS";
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset EffectiveTo { get; set; }
}
