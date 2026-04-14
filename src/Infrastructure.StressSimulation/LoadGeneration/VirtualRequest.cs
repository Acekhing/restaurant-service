using Infrastructure.StressSimulation.Pipeline;

namespace Infrastructure.StressSimulation.LoadGeneration;

public sealed class VirtualRequest
{
    public required EventType Type { get; init; }
    public required string AggregateId { get; init; }
    public double ResponseLatencyMs { get; set; }
    public bool IsError { get; set; }
}
