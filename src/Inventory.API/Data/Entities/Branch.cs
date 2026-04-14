using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InventoryCore.RowVersion;

namespace Inventory.API.Data.Entities;

public class Branch : BaseEntity, IRowVersioned
{
    public long RowVersion { get; set; }

    [Required]
    public string RetailerType { get; set; } = null!;
    [Required]
    public string RetailerId { get; set; } = null!;

    // Basic Info
    public string? BusinessName { get; set; }

    // Business Info
    public string? BusinessPhoneNumber { get; set; }
    public string? BusinessEmail { get; set; }
    public string? AccountManager { get; set; }

    // Location
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public string? LocationName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Zone { get; set; }
    public string? ZoneId { get; set; }
    public string? MainStation { get; set; }
    public string? MainStationId { get; set; }
    [Column(TypeName = "jsonb")]
    public List<string> Stations { get; set; } = new();
    [Column(TypeName = "jsonb")]
    public List<string> StationsIds { get; set; } = new();

    // Payment & Finance
    [Column(TypeName = "jsonb")]
    public List<PaymentMethod>? PaymentMethods { get; set; }
    [Column(TypeName = "jsonb")]
    public PaymentMethod? PreferredPaymentMethods { get; set; }
    public string? AutoSweepAccount { get; set; }
    public bool AutoSweepEnabled { get; set; } = true;
    public bool HasTakePayment { get; set; }
    public string? FineractAccountId { get; set; }
    public string? FineractClientId { get; set; }
    public string? FineractCommissionAccountId { get; set; }
    public bool HasCommissionServices { get; set; }
    public decimal CommissionPercentage { get; set; }
    public decimal CommissionFlat { get; set; }

    // Operating Hours
    [Column(TypeName = "jsonb")]
    public List<RetailerOpeningDayHour> OpeningDayHours { get; set; } = new();
    [Column(TypeName = "jsonb")]
    public List<DisplayTime>? DisplayTimes { get; set; } = new();
    public bool IsSubscribedToReadyToOpenNotification { get; set; } = true;

    // Status & Visibility
    public string? Status { get; set; }
    public bool IsSetupOnPortal { get; set; }
    public bool IsHidden { get; set; }
    public bool IsDeleted { get; set; }
    public bool TemporaryClosed { get; set; }
    public bool PermanentlyClosed { get; set; }
    public bool IsReadyToServe { get; set; }

    // Classification & Behavior
    public string Classification { get; set; } = "Day-time Quick Retailer";
    public bool IsFranchise { get; set; }

    // Images
    public string? DisplayImage { get; set; }
    public string? RawImageUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? ColorCode { get; set; }
    [Column(TypeName = "jsonb")]
    public List<string>? RestaurantImages { get; set; }

    // Search
    [Column(TypeName = "jsonb")]
    public List<string>? SearchTerms { get; set; }

    // Delivery
    public decimal DeliveryFee { get; set; }
    public bool HasRiderPayout { get; set; }
    public decimal RiderPayoutAmount { get; set; }

    // Service Markup
    public decimal ServiceMarkup { get; set; }
    public string? ServiceMarkupNotes { get; set; }

    // Pharmacy-Specific
    public bool? HasPharmacistOnStandby { get; set; }
    public bool? HasAirCondition { get; set; }

    // Restaurant-Specific
    public string? RestaurantType { get; set; }
    [Column(TypeName = "jsonb")]
    public List<string>? MealTypes { get; set; }
    public string[]? Cuisines { get; set; }
    public string? ActivePaymentMethod { get; set; }
    public decimal? ServiceCharge { get; set; }
}
