using Infrastructure.StressSimulation.Configuration;
using Infrastructure.StressSimulation.Pipeline;
using Infrastructure.StressSimulation.Simulators;

namespace Infrastructure.StressSimulation.LoadGeneration;

public sealed class ApiLoadGenerator : SimulatorBase
{
    private readonly Random _rng = new(100);
    private long _requestCount;

    public long RequestCount => _requestCount;

    public ApiLoadGenerator() : base("API Gateway") { }

    /// <summary>
    /// Returns aggregated load counts for the tick instead of individual events.
    /// This avoids allocating millions of objects per tick.
    /// </summary>
    public LoadTickBatch GenerateTickBatch(LoadProfile profile, double elapsedSeconds)
    {
        var availabilityCount = (int)(profile.AvailabilityTogglesPerSec * elapsedSeconds);
        var priceCount = (int)(profile.PriceUpdatesPerSec * elapsedSeconds);
        var branchCount = (int)(profile.BranchUpdatesPerSec * elapsedSeconds);
        var readCount = (long)(profile.ReadsPerSec * elapsedSeconds);
        var menuReadCount = (long)(readCount * 0.4);
        var searchReadCount = readCount - menuReadCount;

        var total = availabilityCount + priceCount + branchCount + readCount;
        Interlocked.Add(ref _requestCount, total);

        return new LoadTickBatch
        {
            AvailabilityToggles = availabilityCount,
            PriceUpdates = priceCount,
            BranchUpdates = branchCount,
            MenuReads = menuReadCount,
            SearchReads = searchReadCount
        };
    }

    public void RecordApiLatency(double ms, bool success)
    {
        if (success)
            RecordLatency(ms);
        else
            RecordError();
    }

    public void RecordBatchLatency(double avgMs, int count, int errors)
    {
        for (var i = 0; i < Math.Min(count, 1000); i++)
            RecordLatency(avgMs + (_rng.NextDouble() - 0.5) * avgMs * 0.3);
        for (var i = 0; i < errors; i++)
            RecordError();
    }

    public override void Reset()
    {
        ResetCounters();
        _requestCount = 0;
    }
}

public sealed class LoadTickBatch
{
    public int AvailabilityToggles { get; init; }
    public int PriceUpdates { get; init; }
    public int BranchUpdates { get; init; }
    public long MenuReads { get; init; }
    public long SearchReads { get; init; }

    public int TotalWrites => AvailabilityToggles + PriceUpdates + BranchUpdates;
    public long TotalReads => MenuReads + SearchReads;
}
