using System.Text.Json;
using Confluent.Kafka;
using Elastic.Clients.Elasticsearch;
using Inventory.Contracts.Outbox;
using Inventory.Contracts.ReadModel;
using InventoryCore.Idempotency;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Inventory.ElasticProjector;

public sealed class ElasticProjectorWorker : BackgroundService
{
    private const string EsIndexName = "inventory";
    private const string MenuEsIndexName = "menus";
    private const string BranchEsIndexName = "branches";

    private static readonly HashSet<string> MenuAggregateTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Menu"
    };

    private static readonly HashSet<string> VarietyAggregateTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Variety"
    };

    private static readonly HashSet<string> InventoryItemAggregateTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "InventoryItem"
    };

    private static readonly HashSet<string> BranchAggregateTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Branch"
    };

    private static readonly HashSet<string> SkippedAggregateTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "InventoryAvailability"
    };

    private readonly ILogger<ElasticProjectorWorker> _logger;
    private readonly IIdempotencyStore _idempotency;
    private readonly ElasticsearchClient _es;
    private readonly NpgsqlDataSource _db;
    private readonly KafkaConsumerOptions _kafka;

    public ElasticProjectorWorker(
        ILogger<ElasticProjectorWorker> logger,
        IIdempotencyStore idempotency,
        ElasticsearchClient es,
        NpgsqlDataSource db,
        IOptions<KafkaConsumerOptions> kafka)
    {
        _logger = logger;
        _idempotency = idempotency;
        _es = es;
        _db = db;
        _kafka = kafka.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cfg = new ConsumerConfig
        {
            BootstrapServers = _kafka.BootstrapServers,
            GroupId = _kafka.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(cfg).Build();
        consumer.Subscribe(_kafka.Topics);
        _logger.LogInformation("Subscribed to {Topics}", string.Join(", ", _kafka.Topics));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var cr = consumer.Consume(TimeSpan.FromSeconds(1));
                if (cr?.Message?.Value is null)
                    continue;

                await HandleAsync(cr.Message.Key, cr.Message.Value, stoppingToken).ConfigureAwait(false);
                consumer.Commit(cr);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error");
            }
        }
    }

    private async Task HandleAsync(string? key, string json, CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (!root.TryGetProperty("after", out var after) || after.ValueKind == JsonValueKind.Null)
            return;

        var outboxId = after.TryGetProperty("id", out var idEl)
            ? idEl.GetGuid()
            : Guid.Parse(key ?? throw new InvalidOperationException("Missing outbox id"));

        if (!await _idempotency.TryAcquireAsync(outboxId.ToString(), TimeSpan.FromHours(24), ct).ConfigureAwait(false))
        {
            _logger.LogDebug("Skip duplicate outbox {Id}", outboxId);
            return;
        }

        var aggregateId = after.GetProperty("aggregate_id").GetString()!;
        var aggregateType = after.TryGetProperty("aggregate_type", out var atEl) ? atEl.GetString() : null;
        var eventType = after.TryGetProperty("event_type", out var etEl) ? etEl.GetString() : null;

        if (aggregateType is not null && SkippedAggregateTypes.Contains(aggregateType))
        {
            _logger.LogDebug("Skipping legacy {AggregateType} outbox event for {AggregateId}", aggregateType, aggregateId);
            return;
        }

        if (aggregateType is not null && MenuAggregateTypes.Contains(aggregateType))
        {
            await HandleMenuAsync(aggregateId, ct).ConfigureAwait(false);
        }
        else if (aggregateType is not null && VarietyAggregateTypes.Contains(aggregateType))
        {
            var inventoryItemIds = await ResolveVarietyItemIdsAsync(aggregateId, ct).ConfigureAwait(false);
            foreach (var itemId in inventoryItemIds)
                await HandleInventoryAsync(itemId, ct).ConfigureAwait(false);
        }
        else if (aggregateType is not null && BranchAggregateTypes.Contains(aggregateType))
        {
            await HandleBranchAsync(aggregateId, ct).ConfigureAwait(false);
        }
        else if (aggregateType is not null && InventoryItemAggregateTypes.Contains(aggregateType))
        {
            var payloadJson = after.TryGetProperty("payload", out var pEl) ? pEl.GetString() : null;
            await HandleInventoryFromPayloadAsync(aggregateId, eventType, payloadJson, ct).ConfigureAwait(false);
        }
        else
        {
            await HandleInventoryAsync(aggregateId, ct).ConfigureAwait(false);
        }
    }

    private async Task HandleInventoryAsync(string aggregateId, CancellationToken ct)
    {
        var (readModel, rowVersion) = await FetchReadModelAsync(aggregateId, ct).ConfigureAwait(false);

        if (readModel is null)
        {
            var deleteResp = await _es.DeleteAsync<InventoryReadModel>(EsIndexName, aggregateId, ct).ConfigureAwait(false);
            if (deleteResp.IsValidResponse)
                _logger.LogInformation("Deleted ES doc for removed item {AggregateId}", aggregateId);
            else
                _logger.LogDebug("ES delete for {AggregateId}: {Debug}", aggregateId, deleteResp.DebugInformation);
            return;
        }

        var resp = await _es.IndexAsync(readModel, i => i
                .Index(EsIndexName)
                .Id(aggregateId)
                .Version(rowVersion)
                .VersionType(VersionType.External), ct)
            .ConfigureAwait(false);

        if (!resp.IsValidResponse)
            _logger.LogWarning("ES index issue for {AggregateId}: {Debug}", aggregateId, resp.DebugInformation);
        else
            _logger.LogDebug("Upserted ES doc for {AggregateId} at version {Version}", aggregateId, rowVersion);
    }

    private async Task HandleInventoryFromPayloadAsync(
        string aggregateId, string? eventType, string? payloadJson, CancellationToken ct)
    {
        if (eventType == "ItemDeleted" || payloadJson is null)
        {
            var deleteResp = await _es.DeleteAsync<InventoryReadModel>(EsIndexName, aggregateId, ct).ConfigureAwait(false);
            if (deleteResp.IsValidResponse)
                _logger.LogInformation("Deleted ES doc for removed item {AggregateId}", aggregateId);
            else
                _logger.LogDebug("ES delete for {AggregateId}: {Debug}", aggregateId, deleteResp.DebugInformation);
            return;
        }

        var payload = JsonSerializer.Deserialize<UnifiedAuditPayload>(payloadJson, CamelCase);
        if (payload?.After is null)
        {
            _logger.LogWarning("Outbox payload has no After snapshot for {AggregateId}", aggregateId);
            return;
        }

        var (readModel, rowVersion) = MapPayloadToReadModel(aggregateId, payload.After);
        readModel.Variety = await FetchVarietyJsonAsync(aggregateId, ct).ConfigureAwait(false);

        var resp = await _es.IndexAsync(readModel, i => i
                .Index(EsIndexName)
                .Id(aggregateId)
                .Version(rowVersion)
                .VersionType(VersionType.External), ct)
            .ConfigureAwait(false);

        if (!resp.IsValidResponse)
            _logger.LogWarning("ES index issue for {AggregateId}: {Debug}", aggregateId, resp.DebugInformation);
        else
            _logger.LogDebug("Upserted ES doc for {AggregateId} at version {Version} (from payload)", aggregateId, rowVersion);
    }

    private static (InventoryReadModel Model, long RowVersion) MapPayloadToReadModel(
        string aggregateId, Dictionary<string, object?> after)
    {
        var model = new InventoryReadModel
        {
            Id = aggregateId,
            Name = GetString(after, "Name") ?? "",
            ShortName = GetString(after, "ShortName"),
            ItemType = GetString(after, "ItemType") ?? "",
            Tags = GetString(after, "Tags"),
            Notes = GetString(after, "Notes"),
            Image = GetString(after, "DefaultImageUrl"),
            RawImageUrl = GetString(after, "RawImageUrl"),
            IsOriginalImage = GetBool(after, "IsOriginalImage"),
            DisplayPrice = GetDecimal(after, "DisplayPrice"),
            SupplierPrice = GetNullableDecimal(after, "SupplierPrice"),
            OldSellingPrice = GetNullableDecimal(after, "WasDisplayPrice"),
            DeliveryFee = GetDecimal(after, "DeliveryFee"),
            PriceRange = GetString(after, "PriceRange"),
            HasDeals = GetBool(after, "HasDeals"),
            DisplayCurrency = GetString(after, "DisplayCurrency") ?? "",
            IsAvailable = GetBool(after, "IsAvailable"),
            OutOfStock = GetBool(after, "OutOfStock"),
            OpeningDayHours = GetOpeningHours(after, "OpeningDayHours"),
            DisplayTimes = GetOpeningHours(after, "DisplayTimes"),
            RetailerId = GetString(after, "RetailerId") ?? "",
            RetailerType = GetString(after, "RetailerType") ?? "",
            HasVariety = GetBool(after, "HasVariety"),
            Variety = null,
            StationId = GetString(after, "StationId"),
            ZoneId = GetString(after, "ZoneId"),
            AveragePreparationTime = GetNullableInt(after, "AveragePreparationTime"),
        };

        var rowVersion = GetLong(after, "RowVersion");
        return (model, rowVersion);
    }

    private static string? GetString(Dictionary<string, object?> d, string key) =>
        d.TryGetValue(key, out var v) && v is JsonElement el && el.ValueKind == JsonValueKind.String
            ? el.GetString()
            : v?.ToString();

    private static bool GetBool(Dictionary<string, object?> d, string key) =>
        d.TryGetValue(key, out var v) && v is JsonElement el && el.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? el.GetBoolean()
            : false;

    private static decimal GetDecimal(Dictionary<string, object?> d, string key) =>
        d.TryGetValue(key, out var v) && v is JsonElement el && el.ValueKind == JsonValueKind.Number
            ? el.GetDecimal()
            : 0m;

    private static decimal? GetNullableDecimal(Dictionary<string, object?> d, string key) =>
        d.TryGetValue(key, out var v) && v is JsonElement el && el.ValueKind == JsonValueKind.Number
            ? el.GetDecimal()
            : null;

    private static int? GetNullableInt(Dictionary<string, object?> d, string key) =>
        d.TryGetValue(key, out var v) && v is JsonElement el && el.ValueKind == JsonValueKind.Number
            ? el.GetInt32()
            : null;

    private static long GetLong(Dictionary<string, object?> d, string key) =>
        d.TryGetValue(key, out var v) && v is JsonElement el && el.ValueKind == JsonValueKind.Number
            ? el.GetInt64()
            : 0;

    private static List<InventoryOpeningHours>? GetOpeningHours(Dictionary<string, object?> d, string key)
    {
        if (!d.TryGetValue(key, out var v) || v is not JsonElement el)
            return null;
        if (el.ValueKind == JsonValueKind.Null)
            return null;
        return JsonSerializer.Deserialize<List<InventoryOpeningHours>>(el.GetRawText(), CamelCase);
    }

    private async Task HandleMenuAsync(string aggregateId, CancellationToken ct)
    {
        var (readModel, rowVersion) = await FetchMenuReadModelAsync(aggregateId, ct).ConfigureAwait(false);

        if (readModel is null)
        {
            var deleteResp = await _es.DeleteAsync<MenuReadModel>(MenuEsIndexName, aggregateId, ct).ConfigureAwait(false);
            if (deleteResp.IsValidResponse)
                _logger.LogInformation("Deleted ES menu doc for {AggregateId}", aggregateId);
            else
                _logger.LogDebug("ES menu delete for {AggregateId}: {Debug}", aggregateId, deleteResp.DebugInformation);
            return;
        }

        var resp = await _es.IndexAsync(readModel, i => i
                .Index(MenuEsIndexName)
                .Id(aggregateId)
                .Version(rowVersion)
                .VersionType(VersionType.External), ct)
            .ConfigureAwait(false);

        if (!resp.IsValidResponse)
            _logger.LogWarning("ES menu index issue for {AggregateId}: {Debug}", aggregateId, resp.DebugInformation);
        else
            _logger.LogDebug("Upserted ES menu doc for {AggregateId} at version {Version}", aggregateId, rowVersion);
    }

    private async Task<List<string>> ResolveVarietyItemIdsAsync(string varietyId, CancellationToken ct)
    {
        const string sql = "SELECT inventory_item_ids FROM variety WHERE id = @id";
        await using var conn = await _db.OpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", varietyId);
        var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        if (result is string json)
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        return [];
    }

    private async Task<string?> FetchVarietyJsonAsync(string inventoryItemId, CancellationToken ct)
    {
        const string sql = """
            SELECT json_build_object(
                'id', v.id,
                'name', v.name,
                'inventoryItemIds', v.inventory_item_ids,
                'varieties', v.varieties,
                'ownerId', v.owner_id,
                'createdAt', v.created_at,
                'updatedAt', v.updated_at
            )
            FROM variety v
            WHERE v.inventory_item_ids @> to_jsonb(@itemId)
            LIMIT 1
            """;

        await using var conn = await _db.OpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("itemId", inventoryItemId);
        var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return result is string json ? json : null;
    }

    private async Task<(InventoryReadModel? Model, long RowVersion)> FetchReadModelAsync(
        string aggregateId, CancellationToken ct)
    {
        const string sql = """
            SELECT
                iv.id, iv.name, iv.short_name, iv.item_type, iv.tags, iv.notes,
                iv.image, iv.raw_image_url, iv.is_original_image,
                iv.display_price, iv.supplier_price, iv.old_selling_price,
                iv.delivery_fee, iv.price_range,
                iv.has_deals, iv.display_currency,
                iv.average_preparation_time,
                iv.has_variety,
                iv.variety,
                iv.is_available, iv.out_of_stock,
                iv.opening_day_hours,
                iv.display_times,
                iv.retailer_id,
                iv.retailer_type,
                iv.station_id, iv.zone_id,
                ii.row_version
            FROM inventory_view iv
            INNER JOIN inventory_item ii ON ii.id = iv.id
            WHERE iv.id = @aggregateId
            """;

        await using var conn = await _db.OpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("aggregateId", aggregateId);

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
            return (null, 0);

        var model = new InventoryReadModel
        {
            Id = reader.GetString(reader.GetOrdinal("id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            ShortName = reader.IsDBNull(reader.GetOrdinal("short_name")) ? null : reader.GetString(reader.GetOrdinal("short_name")),
            ItemType = reader.GetString(reader.GetOrdinal("item_type")),
            Tags = reader.IsDBNull(reader.GetOrdinal("tags")) ? null : reader.GetString(reader.GetOrdinal("tags")),
            Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
            Image = reader.IsDBNull(reader.GetOrdinal("image")) ? null : reader.GetString(reader.GetOrdinal("image")),
            RawImageUrl = reader.IsDBNull(reader.GetOrdinal("raw_image_url")) ? null : reader.GetString(reader.GetOrdinal("raw_image_url")),
            IsOriginalImage = reader.GetBoolean(reader.GetOrdinal("is_original_image")),
            DisplayPrice = reader.GetDecimal(reader.GetOrdinal("display_price")),
            SupplierPrice = reader.IsDBNull(reader.GetOrdinal("supplier_price")) ? null : reader.GetDecimal(reader.GetOrdinal("supplier_price")),
            OldSellingPrice = reader.IsDBNull(reader.GetOrdinal("old_selling_price")) ? null : reader.GetDecimal(reader.GetOrdinal("old_selling_price")),
            DeliveryFee = reader.GetDecimal(reader.GetOrdinal("delivery_fee")),
            PriceRange = reader.IsDBNull(reader.GetOrdinal("price_range")) ? null : reader.GetString(reader.GetOrdinal("price_range")),
            HasDeals = reader.GetBoolean(reader.GetOrdinal("has_deals")),
            DisplayCurrency = reader.GetString(reader.GetOrdinal("display_currency")),
            AveragePreparationTime = reader.IsDBNull(reader.GetOrdinal("average_preparation_time")) ? null : reader.GetInt32(reader.GetOrdinal("average_preparation_time")),
            HasVariety = reader.GetBoolean(reader.GetOrdinal("has_variety")),
            IsAvailable = reader.GetBoolean(reader.GetOrdinal("is_available")),
            OutOfStock = reader.GetBoolean(reader.GetOrdinal("out_of_stock")),
            OpeningDayHours = reader.IsDBNull(reader.GetOrdinal("opening_day_hours")) ? null : JsonSerializer.Deserialize<List<InventoryOpeningHours>>(reader.GetString(reader.GetOrdinal("opening_day_hours")), CamelCase),
            Variety = reader.IsDBNull(reader.GetOrdinal("variety")) ? null : reader.GetString(reader.GetOrdinal("variety")),
            DisplayTimes = reader.IsDBNull(reader.GetOrdinal("display_times")) ? null : JsonSerializer.Deserialize<List<InventoryOpeningHours>>(reader.GetString(reader.GetOrdinal("display_times")), CamelCase),
            RetailerId = reader.GetString(reader.GetOrdinal("retailer_id")),
            RetailerType = reader.IsDBNull(reader.GetOrdinal("retailer_type")) ? "" : reader.GetString(reader.GetOrdinal("retailer_type")),
            StationId = reader.IsDBNull(reader.GetOrdinal("station_id")) ? null : reader.GetString(reader.GetOrdinal("station_id")),
            ZoneId = reader.IsDBNull(reader.GetOrdinal("zone_id")) ? null : reader.GetString(reader.GetOrdinal("zone_id")),
        };

        var rowVersion = reader.GetInt64(reader.GetOrdinal("row_version"));
        return (model, rowVersion);
    }

    private static readonly JsonSerializerOptions CamelCase = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };

    private async Task<(MenuReadModel? Model, long RowVersion)> FetchMenuReadModelAsync(
        string aggregateId, CancellationToken ct)
    {
        const string sql = """
            SELECT
                mv.id, mv.description,
                mv.owner_id, mv.owner_name, mv.owner_image,
                mv.image, mv.is_active,
                mv.display_currency,
                mv.category_id, mv.category_name,
                mv.is_published, mv.is_scheduled, mv.published_at,
                mv.items_json, mv.price_range,
                mv.created_at, mv.updated_at,
                mv.row_version
            FROM menu_view mv
            WHERE mv.id = @aggregateId
            """;

        await using var conn = await _db.OpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("aggregateId", aggregateId);

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
            return (null, 0);

        var itemsJson = reader.IsDBNull(reader.GetOrdinal("items_json")) ? null : reader.GetString(reader.GetOrdinal("items_json"));

        var model = new MenuReadModel
        {
            Id = reader.GetString(reader.GetOrdinal("id")),
            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
            OwnerId = reader.GetString(reader.GetOrdinal("owner_id")),
            OwnerName = reader.IsDBNull(reader.GetOrdinal("owner_name")) ? null : reader.GetString(reader.GetOrdinal("owner_name")),
            OwnerImage = reader.IsDBNull(reader.GetOrdinal("owner_image")) ? null : reader.GetString(reader.GetOrdinal("owner_image")),
            Image = reader.IsDBNull(reader.GetOrdinal("image")) ? null : reader.GetString(reader.GetOrdinal("image")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
            DisplayCurrency = reader.GetString(reader.GetOrdinal("display_currency")),
            CategoryId = reader.IsDBNull(reader.GetOrdinal("category_id")) ? null : reader.GetString(reader.GetOrdinal("category_id")),
            CategoryName = reader.IsDBNull(reader.GetOrdinal("category_name")) ? null : reader.GetString(reader.GetOrdinal("category_name")),
            IsPublished = reader.GetBoolean(reader.GetOrdinal("is_published")),
            IsScheduled = reader.GetBoolean(reader.GetOrdinal("is_scheduled")),
            PublishedAt = reader.IsDBNull(reader.GetOrdinal("published_at")) ? null : reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("published_at")),
            InventoryItems = itemsJson is not null
                ? JsonSerializer.Deserialize<List<MenuInventoryItem>>(itemsJson, CamelCase)
                : null,
            PriceRange = reader.IsDBNull(reader.GetOrdinal("price_range")) ? null : reader.GetString(reader.GetOrdinal("price_range")),
            CreatedAt = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("created_at")),
            UpdatedAt = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("updated_at")),
        };

        var rowVersion = reader.GetInt64(reader.GetOrdinal("row_version"));
        return (model, rowVersion);
    }
    private async Task HandleBranchAsync(string aggregateId, CancellationToken ct)
    {
        var (readModel, rowVersion) = await FetchBranchReadModelAsync(aggregateId, ct).ConfigureAwait(false);

        if (readModel is null)
        {
            var deleteResp = await _es.DeleteAsync<BranchReadModel>(BranchEsIndexName, aggregateId, ct).ConfigureAwait(false);
            if (deleteResp.IsValidResponse)
                _logger.LogInformation("Deleted ES branch doc for {AggregateId}", aggregateId);
            else
                _logger.LogDebug("ES branch delete for {AggregateId}: {Debug}", aggregateId, deleteResp.DebugInformation);
            return;
        }

        var resp = await _es.IndexAsync(readModel, i => i
                .Index(BranchEsIndexName)
                .Id(aggregateId)
                .Version(rowVersion)
                .VersionType(VersionType.External), ct)
            .ConfigureAwait(false);

        if (!resp.IsValidResponse)
            _logger.LogWarning("ES branch index issue for {AggregateId}: {Debug}", aggregateId, resp.DebugInformation);
        else
            _logger.LogDebug("Upserted ES branch doc for {AggregateId} at version {Version}", aggregateId, rowVersion);
    }

    private async Task<(BranchReadModel? Model, long RowVersion)> FetchBranchReadModelAsync(
        string aggregateId, CancellationToken ct)
    {
        const string sql = """
            SELECT
                bv.id, bv.retailer_id, bv.retailer_type, bv.retailer_business_name,
                bv.business_name, bv.business_phone_number, bv.business_email, bv.account_manager,
                bv.longitude, bv.latitude, bv.location_name, bv.address, bv.city, bv.zone, bv.zone_id,
                bv.main_station, bv.main_station_id, bv.stations, bv.stations_ids,
                bv.payment_methods, bv.preferred_payment_methods,
                bv.auto_sweep_account, bv.auto_sweep_enabled, bv.has_take_payment,
                bv.fineract_account_id, bv.fineract_client_id, bv.fineract_commission_account_id,
                bv.has_commission_services, bv.commission_percentage, bv.commission_flat,
                bv.opening_day_hours, bv.display_times, bv.is_subscribed_to_ready_to_open_notification,
                bv.status, bv.is_setup_on_portal, bv.is_hidden, bv.is_deleted,
                bv.temporary_closed, bv.permanently_closed, bv.is_ready_to_serve,
                bv.classification, bv.is_franchise,
                bv.display_image, bv.raw_image_url, bv.logo_url, bv.color_code, bv.restaurant_images,
                bv.search_terms,
                bv.delivery_fee, bv.has_rider_payout, bv.rider_payout_amount,
                bv.service_markup, bv.service_markup_notes,
                bv.has_pharmacist_on_standby, bv.has_air_condition,
                bv.restaurant_type, bv.meal_types, bv.cuisines, bv.active_payment_method, bv.service_charge,
                bv.created_at, bv.updated_at,
                b.row_version
            FROM branch_view bv
            INNER JOIN branches b ON b.id = bv.id
            WHERE bv.id = @aggregateId AND bv.is_deleted = false
            """;

        await using var conn = await _db.OpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("aggregateId", aggregateId);

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
            return (null, 0);

        var model = new BranchReadModel
        {
            Id = reader.GetString(reader.GetOrdinal("id")),
            RetailerId = reader.GetString(reader.GetOrdinal("retailer_id")),
            RetailerType = reader.GetString(reader.GetOrdinal("retailer_type")),
            RetailerBusinessName = reader.IsDBNull(reader.GetOrdinal("retailer_business_name")) ? null : reader.GetString(reader.GetOrdinal("retailer_business_name")),
            BusinessName = reader.IsDBNull(reader.GetOrdinal("business_name")) ? null : reader.GetString(reader.GetOrdinal("business_name")),
            BusinessPhoneNumber = reader.IsDBNull(reader.GetOrdinal("business_phone_number")) ? null : reader.GetString(reader.GetOrdinal("business_phone_number")),
            BusinessEmail = reader.IsDBNull(reader.GetOrdinal("business_email")) ? null : reader.GetString(reader.GetOrdinal("business_email")),
            AccountManager = reader.IsDBNull(reader.GetOrdinal("account_manager")) ? null : reader.GetString(reader.GetOrdinal("account_manager")),
            Longitude = reader.GetDouble(reader.GetOrdinal("longitude")),
            Latitude = reader.GetDouble(reader.GetOrdinal("latitude")),
            LocationName = reader.IsDBNull(reader.GetOrdinal("location_name")) ? null : reader.GetString(reader.GetOrdinal("location_name")),
            Address = reader.IsDBNull(reader.GetOrdinal("address")) ? null : reader.GetString(reader.GetOrdinal("address")),
            City = reader.IsDBNull(reader.GetOrdinal("city")) ? null : reader.GetString(reader.GetOrdinal("city")),
            Zone = reader.IsDBNull(reader.GetOrdinal("zone")) ? null : reader.GetString(reader.GetOrdinal("zone")),
            ZoneId = reader.IsDBNull(reader.GetOrdinal("zone_id")) ? null : reader.GetString(reader.GetOrdinal("zone_id")),
            MainStation = reader.IsDBNull(reader.GetOrdinal("main_station")) ? null : reader.GetString(reader.GetOrdinal("main_station")),
            MainStationId = reader.IsDBNull(reader.GetOrdinal("main_station_id")) ? null : reader.GetString(reader.GetOrdinal("main_station_id")),
            Stations = reader.IsDBNull(reader.GetOrdinal("stations")) ? null : reader.GetString(reader.GetOrdinal("stations")),
            StationsIds = reader.IsDBNull(reader.GetOrdinal("stations_ids")) ? null : reader.GetString(reader.GetOrdinal("stations_ids")),
            PaymentMethods = reader.IsDBNull(reader.GetOrdinal("payment_methods")) ? null : reader.GetString(reader.GetOrdinal("payment_methods")),
            PreferredPaymentMethods = reader.IsDBNull(reader.GetOrdinal("preferred_payment_methods")) ? null : reader.GetString(reader.GetOrdinal("preferred_payment_methods")),
            AutoSweepAccount = reader.IsDBNull(reader.GetOrdinal("auto_sweep_account")) ? null : reader.GetString(reader.GetOrdinal("auto_sweep_account")),
            AutoSweepEnabled = reader.GetBoolean(reader.GetOrdinal("auto_sweep_enabled")),
            HasTakePayment = reader.GetBoolean(reader.GetOrdinal("has_take_payment")),
            FineractAccountId = reader.IsDBNull(reader.GetOrdinal("fineract_account_id")) ? null : reader.GetString(reader.GetOrdinal("fineract_account_id")),
            FineractClientId = reader.IsDBNull(reader.GetOrdinal("fineract_client_id")) ? null : reader.GetString(reader.GetOrdinal("fineract_client_id")),
            FineractCommissionAccountId = reader.IsDBNull(reader.GetOrdinal("fineract_commission_account_id")) ? null : reader.GetString(reader.GetOrdinal("fineract_commission_account_id")),
            HasCommissionServices = reader.GetBoolean(reader.GetOrdinal("has_commission_services")),
            CommissionPercentage = reader.GetDecimal(reader.GetOrdinal("commission_percentage")),
            CommissionFlat = reader.GetDecimal(reader.GetOrdinal("commission_flat")),
            OpeningDayHours = reader.IsDBNull(reader.GetOrdinal("opening_day_hours")) ? null : reader.GetString(reader.GetOrdinal("opening_day_hours")),
            DisplayTimes = reader.IsDBNull(reader.GetOrdinal("display_times")) ? null : reader.GetString(reader.GetOrdinal("display_times")),
            IsSubscribedToReadyToOpenNotification = reader.GetBoolean(reader.GetOrdinal("is_subscribed_to_ready_to_open_notification")),
            Status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString(reader.GetOrdinal("status")),
            IsSetupOnPortal = reader.GetBoolean(reader.GetOrdinal("is_setup_on_portal")),
            IsHidden = reader.GetBoolean(reader.GetOrdinal("is_hidden")),
            IsDeleted = reader.GetBoolean(reader.GetOrdinal("is_deleted")),
            TemporaryClosed = reader.GetBoolean(reader.GetOrdinal("temporary_closed")),
            PermanentlyClosed = reader.GetBoolean(reader.GetOrdinal("permanently_closed")),
            IsReadyToServe = reader.GetBoolean(reader.GetOrdinal("is_ready_to_serve")),
            Classification = reader.GetString(reader.GetOrdinal("classification")),
            IsFranchise = reader.GetBoolean(reader.GetOrdinal("is_franchise")),
            DisplayImage = reader.IsDBNull(reader.GetOrdinal("display_image")) ? null : reader.GetString(reader.GetOrdinal("display_image")),
            RawImageUrl = reader.IsDBNull(reader.GetOrdinal("raw_image_url")) ? null : reader.GetString(reader.GetOrdinal("raw_image_url")),
            LogoUrl = reader.IsDBNull(reader.GetOrdinal("logo_url")) ? null : reader.GetString(reader.GetOrdinal("logo_url")),
            ColorCode = reader.IsDBNull(reader.GetOrdinal("color_code")) ? null : reader.GetString(reader.GetOrdinal("color_code")),
            RestaurantImages = reader.IsDBNull(reader.GetOrdinal("restaurant_images")) ? null : reader.GetString(reader.GetOrdinal("restaurant_images")),
            SearchTerms = reader.IsDBNull(reader.GetOrdinal("search_terms")) ? null : reader.GetString(reader.GetOrdinal("search_terms")),
            DeliveryFee = reader.GetDecimal(reader.GetOrdinal("delivery_fee")),
            HasRiderPayout = reader.GetBoolean(reader.GetOrdinal("has_rider_payout")),
            RiderPayoutAmount = reader.GetDecimal(reader.GetOrdinal("rider_payout_amount")),
            ServiceMarkup = reader.GetDecimal(reader.GetOrdinal("service_markup")),
            ServiceMarkupNotes = reader.IsDBNull(reader.GetOrdinal("service_markup_notes")) ? null : reader.GetString(reader.GetOrdinal("service_markup_notes")),
            HasPharmacistOnStandby = reader.IsDBNull(reader.GetOrdinal("has_pharmacist_on_standby")) ? null : reader.GetBoolean(reader.GetOrdinal("has_pharmacist_on_standby")),
            HasAirCondition = reader.IsDBNull(reader.GetOrdinal("has_air_condition")) ? null : reader.GetBoolean(reader.GetOrdinal("has_air_condition")),
            RestaurantType = reader.IsDBNull(reader.GetOrdinal("restaurant_type")) ? null : reader.GetString(reader.GetOrdinal("restaurant_type")),
            MealTypes = reader.IsDBNull(reader.GetOrdinal("meal_types")) ? null : reader.GetString(reader.GetOrdinal("meal_types")),
            Cuisines = reader.IsDBNull(reader.GetOrdinal("cuisines")) ? null : reader.GetFieldValue<string[]>(reader.GetOrdinal("cuisines")),
            ActivePaymentMethod = reader.IsDBNull(reader.GetOrdinal("active_payment_method")) ? null : reader.GetString(reader.GetOrdinal("active_payment_method")),
            ServiceCharge = reader.IsDBNull(reader.GetOrdinal("service_charge")) ? null : reader.GetDecimal(reader.GetOrdinal("service_charge")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? null : reader.GetDateTime(reader.GetOrdinal("updated_at")),
        };

        var rowVersion = reader.GetInt64(reader.GetOrdinal("row_version"));
        return (model, rowVersion);
    }
}

public sealed class KafkaConsumerOptions
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string GroupId { get; set; } = "inventory-elastic-projector";
    public string[] Topics { get; set; } = [];
}
