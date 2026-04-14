namespace Inventory.Contracts.ReadModel;

/// <summary>
/// Denormalized read model for Branch, backed by <c>branch_view</c> and used as the ES document shape for the "branches" index.
/// </summary>
public class BranchReadModel
{
    public string Id { get; set; } = "";
    public string RetailerId { get; set; } = "";
    public string RetailerType { get; set; } = "";
    public string? RetailerBusinessName { get; set; }

    public string? BusinessName { get; set; }
    public string? BusinessPhoneNumber { get; set; }
    public string? BusinessEmail { get; set; }
    public string? AccountManager { get; set; }

    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public string? LocationName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Zone { get; set; }
    public string? ZoneId { get; set; }
    public string? MainStation { get; set; }
    public string? MainStationId { get; set; }
    public string? Stations { get; set; }
    public string? StationsIds { get; set; }

    public string? PaymentMethods { get; set; }
    public string? PreferredPaymentMethods { get; set; }
    public string? AutoSweepAccount { get; set; }
    public bool AutoSweepEnabled { get; set; }
    public bool HasTakePayment { get; set; }
    public string? FineractAccountId { get; set; }
    public string? FineractClientId { get; set; }
    public string? FineractCommissionAccountId { get; set; }
    public bool HasCommissionServices { get; set; }
    public decimal CommissionPercentage { get; set; }
    public decimal CommissionFlat { get; set; }

    public string? OpeningDayHours { get; set; }
    public string? DisplayTimes { get; set; }
    public bool IsSubscribedToReadyToOpenNotification { get; set; }

    public string? Status { get; set; }
    public bool IsSetupOnPortal { get; set; }
    public bool IsHidden { get; set; }
    public bool IsDeleted { get; set; }
    public bool TemporaryClosed { get; set; }
    public bool PermanentlyClosed { get; set; }
    public bool IsReadyToServe { get; set; }

    public string Classification { get; set; } = "";
    public bool IsFranchise { get; set; }

    public string? DisplayImage { get; set; }
    public string? RawImageUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? ColorCode { get; set; }
    public string? RestaurantImages { get; set; }
    public string? SearchTerms { get; set; }

    public decimal DeliveryFee { get; set; }
    public bool HasRiderPayout { get; set; }
    public decimal RiderPayoutAmount { get; set; }

    public decimal ServiceMarkup { get; set; }
    public string? ServiceMarkupNotes { get; set; }

    public bool? HasPharmacistOnStandby { get; set; }
    public bool? HasAirCondition { get; set; }

    public string? RestaurantType { get; set; }
    public string? MealTypes { get; set; }
    public string[]? Cuisines { get; set; }
    public string? ActivePaymentMethod { get; set; }
    public decimal? ServiceCharge { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
