using Inventory.API.Data;
using Inventory.API.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BranchesController : ControllerBase
{
    private readonly InventoryDbContext _db;

    public BranchesController(InventoryDbContext db) => _db = db;

    // ─── List ───────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? retailerId,
        [FromQuery] string? type,
        CancellationToken ct)
    {
        var query = _db.Branches.AsNoTracking().Where(b => !b.IsDeleted);

        if (!string.IsNullOrEmpty(retailerId))
            query = query.Where(b => b.RetailerId == retailerId);
        if (!string.IsNullOrEmpty(type))
            query = query.Where(b => b.RetailerType == type);

        var rows = await query.OrderBy(b => b.BusinessName).ToListAsync(ct);
        return Ok(rows);
    }

    // ─── Get ────────────────────────────────────────────────────────

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken ct)
    {
        var branch = await _db.Branches.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        return branch is null ? NotFound() : Ok(branch);
    }

    // ─── Create ─────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBranchRequest body, CancellationToken ct)
    {
        var retailer = await _db.Retailers.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == body.RetailerId && !r.IsDeleted, ct);

        if (retailer is null)
            return BadRequest(new { error = "Retailer not found." });

        var branch = new Branch
        {
            RetailerId = body.RetailerId,
            RetailerType = retailer.RetailerType,
            Status = "Approved",
            BusinessName = body.BusinessName,
            BusinessPhoneNumber = body.BusinessPhoneNumber,
            BusinessEmail = body.BusinessEmail,
            AccountManager = body.AccountManager,
            Longitude = body.Longitude,
            Latitude = body.Latitude,
            LocationName = body.LocationName,
            Address = body.Address,
            City = body.City,
            Zone = body.Zone,
            ZoneId = body.ZoneId,
            MainStation = body.MainStation,
            MainStationId = body.MainStationId,
            Stations = body.Stations ?? [],
            StationsIds = body.StationsIds ?? [],
            PaymentMethods = body.PaymentMethods,
            PreferredPaymentMethods = body.PreferredPaymentMethods,
            AutoSweepAccount = body.AutoSweepAccount,
            AutoSweepEnabled = body.AutoSweepEnabled,
            HasTakePayment = body.HasTakePayment,
            FineractAccountId = body.FineractAccountId,
            FineractClientId = body.FineractClientId,
            FineractCommissionAccountId = body.FineractCommissionAccountId,
            HasCommissionServices = body.HasCommissionServices,
            CommissionPercentage = body.CommissionPercentage,
            CommissionFlat = body.CommissionFlat,
            OpeningDayHours = body.OpeningDayHours ?? [],
            DisplayTimes = body.DisplayTimes,
            IsSubscribedToReadyToOpenNotification = body.IsSubscribedToReadyToOpenNotification,
            IsSetupOnPortal = body.IsSetupOnPortal,
            IsHidden = body.IsHidden,
            TemporaryClosed = body.TemporaryClosed,
            PermanentlyClosed = body.PermanentlyClosed,
            IsReadyToServe = body.IsReadyToServe,
            Classification = body.Classification ?? "Day-time Quick Retailer",
            IsFranchise = body.IsFranchise,
            DisplayImage = body.DisplayImage,
            RawImageUrl = body.RawImageUrl,
            LogoUrl = body.LogoUrl,
            ColorCode = body.ColorCode,
            RestaurantImages = body.RestaurantImages,
            SearchTerms = body.SearchTerms,
            DeliveryFee = body.DeliveryFee,
            HasRiderPayout = body.HasRiderPayout,
            RiderPayoutAmount = body.RiderPayoutAmount,
            ServiceMarkup = body.ServiceMarkup,
            ServiceMarkupNotes = body.ServiceMarkupNotes,
            HasPharmacistOnStandby = body.HasPharmacistOnStandby,
            HasAirCondition = body.HasAirCondition,
            RestaurantType = body.RestaurantType,
            MealTypes = body.MealTypes,
            Cuisines = body.Cuisines,
            ActivePaymentMethod = body.ActivePaymentMethod,
            ServiceCharge = body.ServiceCharge
        };

        _db.Branches.Add(branch);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Get), new { id = branch.Id }, branch);
    }

    // ─── Update ─────────────────────────────────────────────────────

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateBranchRequest body, CancellationToken ct)
    {
        var b = await _db.Branches.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (b is null) return NotFound();

        if (body.BusinessName is not null) b.BusinessName = body.BusinessName;
        if (body.BusinessPhoneNumber is not null) b.BusinessPhoneNumber = body.BusinessPhoneNumber;
        if (body.BusinessEmail is not null) b.BusinessEmail = body.BusinessEmail;
        if (body.AccountManager is not null) b.AccountManager = body.AccountManager;
        if (body.Longitude.HasValue) b.Longitude = body.Longitude.Value;
        if (body.Latitude.HasValue) b.Latitude = body.Latitude.Value;
        if (body.LocationName is not null) b.LocationName = body.LocationName;
        if (body.Address is not null) b.Address = body.Address;
        if (body.City is not null) b.City = body.City;
        if (body.Zone is not null) b.Zone = body.Zone;
        if (body.ZoneId is not null) b.ZoneId = body.ZoneId;
        if (body.MainStation is not null) b.MainStation = body.MainStation;
        if (body.MainStationId is not null) b.MainStationId = body.MainStationId;
        if (body.Stations is not null) b.Stations = body.Stations;
        if (body.StationsIds is not null) b.StationsIds = body.StationsIds;
        if (body.PaymentMethods is not null) b.PaymentMethods = body.PaymentMethods;
        if (body.PreferredPaymentMethods is not null) b.PreferredPaymentMethods = body.PreferredPaymentMethods;
        if (body.AutoSweepAccount is not null) b.AutoSweepAccount = body.AutoSweepAccount;
        if (body.AutoSweepEnabled.HasValue) b.AutoSweepEnabled = body.AutoSweepEnabled.Value;
        if (body.HasTakePayment.HasValue) b.HasTakePayment = body.HasTakePayment.Value;
        if (body.FineractAccountId is not null) b.FineractAccountId = body.FineractAccountId;
        if (body.FineractClientId is not null) b.FineractClientId = body.FineractClientId;
        if (body.FineractCommissionAccountId is not null) b.FineractCommissionAccountId = body.FineractCommissionAccountId;
        if (body.HasCommissionServices.HasValue) b.HasCommissionServices = body.HasCommissionServices.Value;
        if (body.CommissionPercentage.HasValue) b.CommissionPercentage = body.CommissionPercentage.Value;
        if (body.CommissionFlat.HasValue) b.CommissionFlat = body.CommissionFlat.Value;
        if (body.OpeningDayHours is not null) b.OpeningDayHours = body.OpeningDayHours;
        if (body.DisplayTimes is not null) b.DisplayTimes = body.DisplayTimes;
        if (body.IsSubscribedToReadyToOpenNotification.HasValue) b.IsSubscribedToReadyToOpenNotification = body.IsSubscribedToReadyToOpenNotification.Value;
        if (body.IsSetupOnPortal.HasValue) b.IsSetupOnPortal = body.IsSetupOnPortal.Value;
        if (body.IsHidden.HasValue) b.IsHidden = body.IsHidden.Value;
        if (body.TemporaryClosed.HasValue) b.TemporaryClosed = body.TemporaryClosed.Value;
        if (body.PermanentlyClosed.HasValue) b.PermanentlyClosed = body.PermanentlyClosed.Value;
        if (body.IsReadyToServe.HasValue) b.IsReadyToServe = body.IsReadyToServe.Value;
        if (body.Classification is not null) b.Classification = body.Classification;
        if (body.IsFranchise.HasValue) b.IsFranchise = body.IsFranchise.Value;
        if (body.DisplayImage is not null) b.DisplayImage = body.DisplayImage;
        if (body.RawImageUrl is not null) b.RawImageUrl = body.RawImageUrl;
        if (body.LogoUrl is not null) b.LogoUrl = body.LogoUrl;
        if (body.ColorCode is not null) b.ColorCode = body.ColorCode;
        if (body.RestaurantImages is not null) b.RestaurantImages = body.RestaurantImages;
        if (body.SearchTerms is not null) b.SearchTerms = body.SearchTerms;
        if (body.DeliveryFee.HasValue) b.DeliveryFee = body.DeliveryFee.Value;
        if (body.HasRiderPayout.HasValue) b.HasRiderPayout = body.HasRiderPayout.Value;
        if (body.RiderPayoutAmount.HasValue) b.RiderPayoutAmount = body.RiderPayoutAmount.Value;
        if (body.ServiceMarkup.HasValue) b.ServiceMarkup = body.ServiceMarkup.Value;
        if (body.ServiceMarkupNotes is not null) b.ServiceMarkupNotes = body.ServiceMarkupNotes;
        if (body.HasPharmacistOnStandby.HasValue) b.HasPharmacistOnStandby = body.HasPharmacistOnStandby.Value;
        if (body.HasAirCondition.HasValue) b.HasAirCondition = body.HasAirCondition.Value;
        if (body.RestaurantType is not null) b.RestaurantType = body.RestaurantType;
        if (body.MealTypes is not null) b.MealTypes = body.MealTypes;
        if (body.Cuisines is not null) b.Cuisines = body.Cuisines;
        if (body.ActivePaymentMethod is not null) b.ActivePaymentMethod = body.ActivePaymentMethod;
        if (body.ServiceCharge.HasValue) b.ServiceCharge = body.ServiceCharge.Value;

        b.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(b);
    }

    // ─── Delete (soft) ──────────────────────────────────────────────

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var b = await _db.Branches.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (b is null) return NotFound();

        b.IsDeleted = true;
        b.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

// ─── Request DTOs ───────────────────────────────────────────────────

public sealed class CreateBranchRequest
{
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
    public List<string>? Stations { get; set; }
    public List<string>? StationsIds { get; set; }

    // Payment & Finance
    public List<PaymentMethod>? PaymentMethods { get; set; }
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
    public List<RetailerOpeningDayHour>? OpeningDayHours { get; set; }
    public List<DisplayTime>? DisplayTimes { get; set; }
    public bool IsSubscribedToReadyToOpenNotification { get; set; } = true;

    // Status & Visibility
    public bool IsSetupOnPortal { get; set; }
    public bool IsHidden { get; set; }
    public bool TemporaryClosed { get; set; }
    public bool PermanentlyClosed { get; set; }
    public bool IsReadyToServe { get; set; }

    // Classification & Behavior
    public string? Classification { get; set; }
    public bool IsFranchise { get; set; }

    // Images
    public string? DisplayImage { get; set; }
    public string? RawImageUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? ColorCode { get; set; }
    public List<string>? RestaurantImages { get; set; }
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
    public List<string>? MealTypes { get; set; }
    public string[]? Cuisines { get; set; }
    public string? ActivePaymentMethod { get; set; }
    public decimal? ServiceCharge { get; set; }
}

public sealed class UpdateBranchRequest
{
    // Basic Info
    public string? BusinessName { get; set; }

    // Business Info
    public string? BusinessPhoneNumber { get; set; }
    public string? BusinessEmail { get; set; }
    public string? AccountManager { get; set; }

    // Location
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
    public string? LocationName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Zone { get; set; }
    public string? ZoneId { get; set; }
    public string? MainStation { get; set; }
    public string? MainStationId { get; set; }
    public List<string>? Stations { get; set; }
    public List<string>? StationsIds { get; set; }

    // Payment & Finance
    public List<PaymentMethod>? PaymentMethods { get; set; }
    public PaymentMethod? PreferredPaymentMethods { get; set; }
    public string? AutoSweepAccount { get; set; }
    public bool? AutoSweepEnabled { get; set; }
    public bool? HasTakePayment { get; set; }
    public string? FineractAccountId { get; set; }
    public string? FineractClientId { get; set; }
    public string? FineractCommissionAccountId { get; set; }
    public bool? HasCommissionServices { get; set; }
    public decimal? CommissionPercentage { get; set; }
    public decimal? CommissionFlat { get; set; }

    // Operating Hours
    public List<RetailerOpeningDayHour>? OpeningDayHours { get; set; }
    public List<DisplayTime>? DisplayTimes { get; set; }
    public bool? IsSubscribedToReadyToOpenNotification { get; set; }

    // Status & Visibility
    public bool? IsSetupOnPortal { get; set; }
    public bool? IsHidden { get; set; }
    public bool? TemporaryClosed { get; set; }
    public bool? PermanentlyClosed { get; set; }
    public bool? IsReadyToServe { get; set; }

    // Classification & Behavior
    public string? Classification { get; set; }
    public bool? IsFranchise { get; set; }

    // Images
    public string? DisplayImage { get; set; }
    public string? RawImageUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? ColorCode { get; set; }
    public List<string>? RestaurantImages { get; set; }
    public List<string>? SearchTerms { get; set; }

    // Delivery
    public decimal? DeliveryFee { get; set; }
    public bool? HasRiderPayout { get; set; }
    public decimal? RiderPayoutAmount { get; set; }

    // Service Markup
    public decimal? ServiceMarkup { get; set; }
    public string? ServiceMarkupNotes { get; set; }

    // Pharmacy-Specific
    public bool? HasPharmacistOnStandby { get; set; }
    public bool? HasAirCondition { get; set; }

    // Restaurant-Specific
    public string? RestaurantType { get; set; }
    public List<string>? MealTypes { get; set; }
    public string[]? Cuisines { get; set; }
    public string? ActivePaymentMethod { get; set; }
    public decimal? ServiceCharge { get; set; }
}
