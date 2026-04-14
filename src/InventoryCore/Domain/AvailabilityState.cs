namespace InventoryCore.Domain;

/// <summary>
/// Canonical availability state across verticals (shared vocabulary; transitions enforced in vertical services).
/// </summary>
public enum AvailabilityState
{
    Unknown = 0,
    Available = 1,
    Unavailable = 2,
    Hidden = 3,
    OutOfStock = 4
}
