using Inventory.API.Data;
using Inventory.API.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RetailersController : ControllerBase
{
    private readonly InventoryDbContext _db;

    public RetailersController(InventoryDbContext db) => _db = db;

    private static readonly string[] ValidTypes = ["restaurant", "pharmacy", "shop"];

    // ─── List ───────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? type, CancellationToken ct)
    {
        var query = _db.Retailers.AsNoTracking().Where(r => !r.IsDeleted);

        if (!string.IsNullOrEmpty(type))
            query = query.Where(r => r.RetailerType == type);

        var rows = await query.OrderBy(r => r.BusinessName).ToListAsync(ct);
        return Ok(rows);
    }

    // ─── Get ────────────────────────────────────────────────────────

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken ct)
    {
        var retailer = await _db.Retailers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        return retailer is null ? NotFound() : Ok(retailer);
    }

    // ─── Create ─────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRetailerRequest body, CancellationToken ct)
    {
        if (!ValidTypes.Contains(body.RetailerType))
            return BadRequest(new { error = "Invalid retailer type. Must be restaurant, pharmacy, or shop." });

        var retailer = new Retailer
        {
            RetailerType = body.RetailerType,
            Status = "Approved",
            BusinessName = body.BusinessName,
            Notes = body.Notes,
            BusinessPhoneNumber = body.BusinessPhoneNumber,
            BusinessEmail = body.BusinessEmail,
            AccountManager = body.AccountManager,
            ComplianceId = body.ComplianceId,
            OrderTelephoneNumbers = body.OrderTelephoneNumbers ?? [],
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
            Country = body.Country,
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
            RetailerAgreement = body.RetailerAgreement,
            SocialMediaLinks = body.SocialMediaLinks,
            HasPharmacistOnStandby = body.HasPharmacistOnStandby,
            HasAirCondition = body.HasAirCondition,
            RestaurantType = body.RestaurantType,
            MealTypes = body.MealTypes,
            Cuisines = body.Cuisines,
            PreparationStyle = body.PreparationStyle,
            MealPackaging = body.MealPackaging,
            ActivePaymentMethod = body.ActivePaymentMethod,
            ServiceCharge = body.ServiceCharge
        };

        _db.Retailers.Add(retailer);

        var defaultBranch = new Branch
        {
            RetailerId = retailer.Id,
            RetailerType = retailer.RetailerType,
            Status = "Active",
            BusinessName = retailer.BusinessName,
            BusinessPhoneNumber = retailer.BusinessPhoneNumber,
            BusinessEmail = retailer.BusinessEmail,
            AccountManager = retailer.AccountManager,
            Longitude = retailer.Longitude,
            Latitude = retailer.Latitude,
            LocationName = retailer.LocationName,
            Address = retailer.Address,
            City = retailer.City,
            Zone = retailer.Zone,
            ZoneId = retailer.ZoneId,
            MainStation = retailer.MainStation,
            MainStationId = retailer.MainStationId,
            Stations = retailer.Stations,
            StationsIds = retailer.StationsIds,
            PaymentMethods = retailer.PaymentMethods,
            PreferredPaymentMethods = retailer.PreferredPaymentMethods,
            AutoSweepAccount = retailer.AutoSweepAccount,
            AutoSweepEnabled = retailer.AutoSweepEnabled,
            HasTakePayment = retailer.HasTakePayment,
            FineractAccountId = retailer.FineractAccountId,
            FineractClientId = retailer.FineractClientId,
            FineractCommissionAccountId = retailer.FineractCommissionAccountId,
            HasCommissionServices = retailer.HasCommissionServices,
            CommissionPercentage = retailer.CommissionPercentage,
            CommissionFlat = retailer.CommissionFlat,
            OpeningDayHours = retailer.OpeningDayHours,
            DisplayTimes = retailer.DisplayTimes,
            IsSubscribedToReadyToOpenNotification = retailer.IsSubscribedToReadyToOpenNotification,
            IsSetupOnPortal = retailer.IsSetupOnPortal,
            IsHidden = retailer.IsHidden,
            TemporaryClosed = retailer.TemporaryClosed,
            PermanentlyClosed = retailer.PermanentlyClosed,
            IsReadyToServe = retailer.IsReadyToServe,
            Classification = retailer.Classification,
            IsFranchise = retailer.IsFranchise,
            DisplayImage = retailer.DisplayImage,
            RawImageUrl = retailer.RawImageUrl,
            LogoUrl = retailer.LogoUrl,
            ColorCode = retailer.ColorCode,
            RestaurantImages = retailer.RestaurantImages,
            SearchTerms = retailer.SearchTerms,
            DeliveryFee = retailer.DeliveryFee,
            HasRiderPayout = retailer.HasRiderPayout,
            RiderPayoutAmount = retailer.RiderPayoutAmount,
            ServiceMarkup = retailer.ServiceMarkup,
            ServiceMarkupNotes = retailer.ServiceMarkupNotes,
            HasPharmacistOnStandby = retailer.HasPharmacistOnStandby,
            HasAirCondition = retailer.HasAirCondition,
            RestaurantType = retailer.RestaurantType,
            MealTypes = retailer.MealTypes,
            Cuisines = retailer.Cuisines,
            ActivePaymentMethod = retailer.ActivePaymentMethod,
            ServiceCharge = retailer.ServiceCharge
        };
        _db.Branches.Add(defaultBranch);

        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Get), new { id = retailer.Id }, retailer);
    }

    // ─── Update ─────────────────────────────────────────────────────

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateRetailerRequest body, CancellationToken ct)
    {
        var r = await _db.Retailers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (r is null) return NotFound();

        if (body.BusinessName is not null) r.BusinessName = body.BusinessName;
        if (body.Notes is not null) r.Notes = body.Notes;
        if (body.BusinessPhoneNumber is not null) r.BusinessPhoneNumber = body.BusinessPhoneNumber;
        if (body.BusinessEmail is not null) r.BusinessEmail = body.BusinessEmail;
        if (body.AccountManager is not null) r.AccountManager = body.AccountManager;
        if (body.ComplianceId is not null) r.ComplianceId = body.ComplianceId;
        if (body.OrderTelephoneNumbers is not null) r.OrderTelephoneNumbers = body.OrderTelephoneNumbers;
        if (body.Longitude.HasValue) r.Longitude = body.Longitude.Value;
        if (body.Latitude.HasValue) r.Latitude = body.Latitude.Value;
        if (body.LocationName is not null) r.LocationName = body.LocationName;
        if (body.Address is not null) r.Address = body.Address;
        if (body.City is not null) r.City = body.City;
        if (body.Zone is not null) r.Zone = body.Zone;
        if (body.ZoneId is not null) r.ZoneId = body.ZoneId;
        if (body.MainStation is not null) r.MainStation = body.MainStation;
        if (body.MainStationId is not null) r.MainStationId = body.MainStationId;
        if (body.Stations is not null) r.Stations = body.Stations;
        if (body.StationsIds is not null) r.StationsIds = body.StationsIds;
        if (body.Country is not null) r.Country = body.Country;
        if (body.PaymentMethods is not null) r.PaymentMethods = body.PaymentMethods;
        if (body.PreferredPaymentMethods is not null) r.PreferredPaymentMethods = body.PreferredPaymentMethods;
        if (body.AutoSweepAccount is not null) r.AutoSweepAccount = body.AutoSweepAccount;
        if (body.AutoSweepEnabled.HasValue) r.AutoSweepEnabled = body.AutoSweepEnabled.Value;
        if (body.HasTakePayment.HasValue) r.HasTakePayment = body.HasTakePayment.Value;
        if (body.FineractAccountId is not null) r.FineractAccountId = body.FineractAccountId;
        if (body.FineractClientId is not null) r.FineractClientId = body.FineractClientId;
        if (body.FineractCommissionAccountId is not null) r.FineractCommissionAccountId = body.FineractCommissionAccountId;
        if (body.HasCommissionServices.HasValue) r.HasCommissionServices = body.HasCommissionServices.Value;
        if (body.CommissionPercentage.HasValue) r.CommissionPercentage = body.CommissionPercentage.Value;
        if (body.CommissionFlat.HasValue) r.CommissionFlat = body.CommissionFlat.Value;
        if (body.OpeningDayHours is not null) r.OpeningDayHours = body.OpeningDayHours;
        if (body.DisplayTimes is not null) r.DisplayTimes = body.DisplayTimes;
        if (body.IsSubscribedToReadyToOpenNotification.HasValue) r.IsSubscribedToReadyToOpenNotification = body.IsSubscribedToReadyToOpenNotification.Value;
        if (body.IsSetupOnPortal.HasValue) r.IsSetupOnPortal = body.IsSetupOnPortal.Value;
        if (body.IsHidden.HasValue) r.IsHidden = body.IsHidden.Value;
        if (body.TemporaryClosed.HasValue) r.TemporaryClosed = body.TemporaryClosed.Value;
        if (body.PermanentlyClosed.HasValue) r.PermanentlyClosed = body.PermanentlyClosed.Value;
        if (body.IsReadyToServe.HasValue) r.IsReadyToServe = body.IsReadyToServe.Value;
        if (body.Classification is not null) r.Classification = body.Classification;
        if (body.IsFranchise.HasValue) r.IsFranchise = body.IsFranchise.Value;
        if (body.DisplayImage is not null) r.DisplayImage = body.DisplayImage;
        if (body.RawImageUrl is not null) r.RawImageUrl = body.RawImageUrl;
        if (body.LogoUrl is not null) r.LogoUrl = body.LogoUrl;
        if (body.ColorCode is not null) r.ColorCode = body.ColorCode;
        if (body.RestaurantImages is not null) r.RestaurantImages = body.RestaurantImages;
        if (body.SearchTerms is not null) r.SearchTerms = body.SearchTerms;
        if (body.DeliveryFee.HasValue) r.DeliveryFee = body.DeliveryFee.Value;
        if (body.HasRiderPayout.HasValue) r.HasRiderPayout = body.HasRiderPayout.Value;
        if (body.RiderPayoutAmount.HasValue) r.RiderPayoutAmount = body.RiderPayoutAmount.Value;
        if (body.ServiceMarkup.HasValue) r.ServiceMarkup = body.ServiceMarkup.Value;
        if (body.ServiceMarkupNotes is not null) r.ServiceMarkupNotes = body.ServiceMarkupNotes;
        if (body.RetailerAgreement is not null) r.RetailerAgreement = body.RetailerAgreement;
        if (body.SocialMediaLinks is not null) r.SocialMediaLinks = body.SocialMediaLinks;
        if (body.HasPharmacistOnStandby.HasValue) r.HasPharmacistOnStandby = body.HasPharmacistOnStandby.Value;
        if (body.HasAirCondition.HasValue) r.HasAirCondition = body.HasAirCondition.Value;
        if (body.RestaurantType is not null) r.RestaurantType = body.RestaurantType;
        if (body.MealTypes is not null) r.MealTypes = body.MealTypes;
        if (body.Cuisines is not null) r.Cuisines = body.Cuisines;
        if (body.PreparationStyle is not null) r.PreparationStyle = body.PreparationStyle;
        if (body.MealPackaging is not null) r.MealPackaging = body.MealPackaging;
        if (body.ActivePaymentMethod is not null) r.ActivePaymentMethod = body.ActivePaymentMethod;
        if (body.ServiceCharge.HasValue) r.ServiceCharge = body.ServiceCharge.Value;

        r.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(r);
    }

    // ─── Delete (soft) ──────────────────────────────────────────────

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var r = await _db.Retailers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (r is null) return NotFound();

        r.IsDeleted = true;
        r.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

// ─── Request DTOs ───────────────────────────────────────────────────

public sealed class CreateRetailerRequest
{
    public string RetailerType { get; set; } = null!;

    // Basic Info
    public string? BusinessName { get; set; }

    // Business Info
    public string? Notes { get; set; }
    public string? BusinessPhoneNumber { get; set; }
    public string? BusinessEmail { get; set; }
    public string? AccountManager { get; set; }
    public string? ComplianceId { get; set; }
    public List<OrderTelephoneNumber>? OrderTelephoneNumbers { get; set; }

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
    public string? Country { get; set; }

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

    // Agreements & Social
    public List<RetailerAgreement>? RetailerAgreement { get; set; }
    public List<SocialMediaLink>? SocialMediaLinks { get; set; }

    // Pharmacy-Specific
    public bool? HasPharmacistOnStandby { get; set; }
    public bool? HasAirCondition { get; set; }

    // Restaurant-Specific
    public string? RestaurantType { get; set; }
    public List<string>? MealTypes { get; set; }
    public string[]? Cuisines { get; set; }
    public List<string>? PreparationStyle { get; set; }
    public List<string>? MealPackaging { get; set; }
    public string? ActivePaymentMethod { get; set; }
    public decimal? ServiceCharge { get; set; }
}

public sealed class UpdateRetailerRequest
{
    // Basic Info
    public string? BusinessName { get; set; }

    // Business Info
    public string? Notes { get; set; }
    public string? BusinessPhoneNumber { get; set; }
    public string? BusinessEmail { get; set; }
    public string? AccountManager { get; set; }
    public string? ComplianceId { get; set; }
    public List<OrderTelephoneNumber>? OrderTelephoneNumbers { get; set; }

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
    public string? Country { get; set; }

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

    // Agreements & Social
    public List<RetailerAgreement>? RetailerAgreement { get; set; }
    public List<SocialMediaLink>? SocialMediaLinks { get; set; }

    // Pharmacy-Specific
    public bool? HasPharmacistOnStandby { get; set; }
    public bool? HasAirCondition { get; set; }

    // Restaurant-Specific
    public string? RestaurantType { get; set; }
    public List<string>? MealTypes { get; set; }
    public string[]? Cuisines { get; set; }
    public List<string>? PreparationStyle { get; set; }
    public List<string>? MealPackaging { get; set; }
    public string? ActivePaymentMethod { get; set; }
    public decimal? ServiceCharge { get; set; }
}
