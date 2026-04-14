namespace Infrastructure.StressSimulation.Configuration;

public sealed class LoadProfile
{
    public long Branches { get; init; } = 30_000;
    public long InventoryItems { get; init; } = 10_000_000;
    public long Menus { get; init; } = 5_000_000;
    public int ConcurrentUsers { get; init; } = 50_000;
    public double AvailabilityTogglesPerMin { get; init; } = 250_000;
    public double PriceUpdatesPerMin { get; init; } = 120_000;
    public double BranchUpdatesPerMin { get; init; } = 5_000;
    public double ReadsPerSec { get; init; } = 500_000;

    public double AvailabilityTogglesPerSec => AvailabilityTogglesPerMin / 60.0;
    public double PriceUpdatesPerSec => PriceUpdatesPerMin / 60.0;
    public double BranchUpdatesPerSec => BranchUpdatesPerMin / 60.0;

    public double TotalWritesPerSec => AvailabilityTogglesPerSec + PriceUpdatesPerSec + BranchUpdatesPerSec;
    public double TotalOutboxEventsPerSec => TotalWritesPerSec * OutboxEventsPerWrite;

    public double OutboxEventsPerWrite { get; init; } = 1.3;

    private const long BaselineBranches = 30_000;
    private const long BaselineInventoryItems = 10_000_000;
    private const int BaselineConcurrentUsers = 50_000;
    private const double BaselineTogglesPerMin = 250_000;
    private const double BaselinePriceUpdatesPerMin = 120_000;
    private const double BaselineBranchUpdatesPerMin = 5_000;
    private const double BaselineReadsPerSec = 500_000;

    public static LoadProfile FromEntityCounts(
        long branches, long inventoryItems, long menus, int concurrentUsers)
    {
        var itemRatio = (double)inventoryItems / BaselineInventoryItems;
        var userRatio = (double)concurrentUsers / BaselineConcurrentUsers;
        var branchRatio = (double)branches / BaselineBranches;

        return new LoadProfile
        {
            Branches = branches,
            InventoryItems = inventoryItems,
            Menus = menus,
            ConcurrentUsers = concurrentUsers,
            AvailabilityTogglesPerMin = BaselineTogglesPerMin * itemRatio,
            PriceUpdatesPerMin = BaselinePriceUpdatesPerMin * itemRatio,
            BranchUpdatesPerMin = BaselineBranchUpdatesPerMin * branchRatio,
            ReadsPerSec = BaselineReadsPerSec * userRatio,
        };
    }

    public LoadProfile Scale(double multiplier) => new()
    {
        Branches = Branches,
        InventoryItems = InventoryItems,
        Menus = Menus,
        ConcurrentUsers = (int)(ConcurrentUsers * multiplier),
        AvailabilityTogglesPerMin = AvailabilityTogglesPerMin * multiplier,
        PriceUpdatesPerMin = PriceUpdatesPerMin * multiplier,
        BranchUpdatesPerMin = BranchUpdatesPerMin * multiplier,
        ReadsPerSec = ReadsPerSec * multiplier,
        OutboxEventsPerWrite = OutboxEventsPerWrite
    };

    public static LoadProfile Default => new();
}
