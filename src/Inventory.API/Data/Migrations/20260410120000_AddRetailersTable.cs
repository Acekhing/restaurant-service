using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Data.Migrations;

/// <inheritdoc />
public partial class AddRetailersTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "retailers",
            columns: table => new
            {
                id = table.Column<string>(type: "text", nullable: false),
                retailer_type = table.Column<string>(type: "text", nullable: false),
                business_name = table.Column<string>(type: "text", nullable: true),
                notes = table.Column<string>(type: "text", nullable: true),
                business_phone_number = table.Column<string>(type: "text", nullable: true),
                business_email = table.Column<string>(type: "text", nullable: true),
                account_manager = table.Column<string>(type: "text", nullable: true),
                compliance_id = table.Column<string>(type: "text", nullable: true),
                order_telephone_numbers = table.Column<string>(type: "jsonb", nullable: false),
                longitude = table.Column<double>(type: "double precision", nullable: false),
                latitude = table.Column<double>(type: "double precision", nullable: false),
                location_name = table.Column<string>(type: "text", nullable: true),
                address = table.Column<string>(type: "text", nullable: true),
                city = table.Column<string>(type: "text", nullable: true),
                zone = table.Column<string>(type: "text", nullable: true),
                zone_id = table.Column<string>(type: "text", nullable: true),
                main_station = table.Column<string>(type: "text", nullable: true),
                main_station_id = table.Column<string>(type: "text", nullable: true),
                stations = table.Column<string>(type: "jsonb", nullable: false),
                stations_ids = table.Column<string>(type: "jsonb", nullable: false),
                country = table.Column<string>(type: "text", nullable: true),
                payment_methods = table.Column<string>(type: "jsonb", nullable: true),
                preferred_payment_methods = table.Column<string>(type: "jsonb", nullable: true),
                auto_sweep_account = table.Column<string>(type: "text", nullable: true),
                auto_sweep_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                has_take_payment = table.Column<bool>(type: "boolean", nullable: false),
                fineract_account_id = table.Column<string>(type: "text", nullable: true),
                fineract_client_id = table.Column<string>(type: "text", nullable: true),
                fineract_commission_account_id = table.Column<string>(type: "text", nullable: true),
                has_commission_services = table.Column<bool>(type: "boolean", nullable: false),
                commission_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                commission_flat = table.Column<decimal>(type: "numeric", nullable: false),
                opening_day_hours = table.Column<string>(type: "jsonb", nullable: false),
                display_times = table.Column<string>(type: "jsonb", nullable: true),
                is_subscribed_to_ready_to_open_notification = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                status = table.Column<string>(type: "text", nullable: true),
                is_setup_on_portal = table.Column<bool>(type: "boolean", nullable: false),
                is_hidden = table.Column<bool>(type: "boolean", nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                temporary_closed = table.Column<bool>(type: "boolean", nullable: false),
                permanently_closed = table.Column<bool>(type: "boolean", nullable: false),
                is_ready_to_serve = table.Column<bool>(type: "boolean", nullable: false),
                classification = table.Column<string>(type: "text", nullable: false, defaultValue: "Day-time Quick Retailer"),
                is_franchise = table.Column<bool>(type: "boolean", nullable: false),
                display_image = table.Column<string>(type: "text", nullable: true),
                raw_image_url = table.Column<string>(type: "text", nullable: true),
                logo_url = table.Column<string>(type: "text", nullable: true),
                color_code = table.Column<string>(type: "text", nullable: true),
                restaurant_images = table.Column<string>(type: "jsonb", nullable: true),
                search_terms = table.Column<string>(type: "jsonb", nullable: true),
                delivery_fee = table.Column<decimal>(type: "numeric", nullable: false),
                has_rider_payout = table.Column<bool>(type: "boolean", nullable: false),
                rider_payout_amount = table.Column<decimal>(type: "numeric", nullable: false),
                service_markup = table.Column<decimal>(type: "numeric", nullable: false),
                service_markup_notes = table.Column<string>(type: "text", nullable: true),
                retailer_agreement = table.Column<string>(type: "jsonb", nullable: true),
                social_media_links = table.Column<string>(type: "jsonb", nullable: true),
                has_pharmacist_on_standby = table.Column<bool>(type: "boolean", nullable: true),
                has_air_condition = table.Column<bool>(type: "boolean", nullable: true),
                restaurant_type = table.Column<string>(type: "text", nullable: true),
                meal_types = table.Column<string>(type: "jsonb", nullable: true),
                cuisines = table.Column<string[]>(type: "text[]", nullable: true),
                preparation_style = table.Column<string>(type: "jsonb", nullable: true),
                meal_packaging = table.Column<string>(type: "jsonb", nullable: true),
                active_payment_method = table.Column<string>(type: "text", nullable: true),
                service_charge = table.Column<decimal>(type: "numeric", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<string>(type: "text", nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                last_updated_by = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_retailers", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_retailers_retailer_type",
            table: "retailers",
            column: "retailer_type");

        migrationBuilder.Sql("""
            INSERT INTO retailers (id, retailer_type, business_name, display_image, created_at,
                order_telephone_numbers, stations, stations_ids, opening_day_hours,
                longitude, latitude, commission_percentage, commission_flat,
                delivery_fee, rider_payout_amount, service_markup,
                has_take_payment, has_commission_services, is_setup_on_portal,
                is_hidden, is_deleted, temporary_closed, permanently_closed,
                is_ready_to_serve, is_franchise, has_rider_payout,
                classification, auto_sweep_enabled, is_subscribed_to_ready_to_open_notification)
            SELECT id, 'restaurant', name, image, NOW(),
                '[]'::jsonb, '[]'::jsonb, '[]'::jsonb, '[]'::jsonb,
                0, 0, 0, 0,
                0, 0, 0,
                false, false, false,
                false, false, false, false,
                false, false, false,
                'Day-time Quick Retailer', true, true
            FROM restaurants;
            """);

        migrationBuilder.Sql("""
            INSERT INTO retailers (id, retailer_type, business_name, display_image, created_at,
                order_telephone_numbers, stations, stations_ids, opening_day_hours,
                longitude, latitude, commission_percentage, commission_flat,
                delivery_fee, rider_payout_amount, service_markup,
                has_take_payment, has_commission_services, is_setup_on_portal,
                is_hidden, is_deleted, temporary_closed, permanently_closed,
                is_ready_to_serve, is_franchise, has_rider_payout,
                classification, auto_sweep_enabled, is_subscribed_to_ready_to_open_notification)
            SELECT id, 'pharmacy', name, image, NOW(),
                '[]'::jsonb, '[]'::jsonb, '[]'::jsonb, '[]'::jsonb,
                0, 0, 0, 0,
                0, 0, 0,
                false, false, false,
                false, false, false, false,
                false, false, false,
                'Day-time Quick Retailer', true, true
            FROM pharmacies;
            """);

        migrationBuilder.Sql("""
            INSERT INTO retailers (id, retailer_type, business_name, display_image, created_at,
                order_telephone_numbers, stations, stations_ids, opening_day_hours,
                longitude, latitude, commission_percentage, commission_flat,
                delivery_fee, rider_payout_amount, service_markup,
                has_take_payment, has_commission_services, is_setup_on_portal,
                is_hidden, is_deleted, temporary_closed, permanently_closed,
                is_ready_to_serve, is_franchise, has_rider_payout,
                classification, auto_sweep_enabled, is_subscribed_to_ready_to_open_notification)
            SELECT id, 'shop', name, image, NOW(),
                '[]'::jsonb, '[]'::jsonb, '[]'::jsonb, '[]'::jsonb,
                0, 0, 0, 0,
                0, 0, 0,
                false, false, false,
                false, false, false, false,
                false, false, false,
                'Day-time Quick Retailer', true, true
            FROM shops;
            """);

        migrationBuilder.Sql("DROP VIEW IF EXISTS menu_view;");

        migrationBuilder.Sql("""
            CREATE OR REPLACE VIEW menu_view AS
            SELECT
                m.id,
                m.description,
                m.owner_id,
                ret.business_name AS owner_name,
                ret.display_image AS owner_image,
                m.image,
                m.display_currency,
                m.is_active,
                m.category_id::text AS category_id,
                c.name AS category_name,
                m.is_published,
                m.is_scheduled,
                m.published_at,
                (
                    SELECT json_agg(json_build_object(
                        'id', i.id,
                        'name', i.name,
                        'displayPrice', i.display_price,
                        'sortOrder', 0,
                        'isRequired', COALESCE((raw_entry->>'IsRequired')::boolean, false),
                        'variety', (
                            SELECT json_build_object(
                                'id', v.id,
                                'name', v.name,
                                'options', v.varieties
                            )
                            FROM variety v
                            WHERE v.inventory_item_ids @> to_jsonb(i.id)
                            LIMIT 1
                        )
                    ))
                    FROM jsonb_array_elements(m.inventory_item_ids) AS raw_entry
                    JOIN inventory_item i ON i.id = raw_entry->>'InventoryItemId' AND i.is_deleted = FALSE
                ) AS items_json,
                CASE
                    WHEN agg.min_price IS NOT NULL AND agg.max_price IS NOT NULL AND agg.min_price <> agg.max_price
                        THEN agg.min_price::text || ' - ' || agg.max_price::text
                    WHEN agg.min_price IS NOT NULL
                        THEN agg.min_price::text
                    ELSE NULL
                END AS price_range,
                m.created_at,
                m.updated_at,
                m.row_version
            FROM menu m
            LEFT JOIN retailers ret ON ret.id = m.owner_id
            LEFT JOIN category c ON c.id = m.category_id
            LEFT JOIN LATERAL (
                SELECT
                    MIN(i.display_price) AS min_price,
                    MAX(i.display_price) AS max_price
                FROM jsonb_array_elements(m.inventory_item_ids) AS raw_entry
                JOIN inventory_item i ON i.id = raw_entry->>'InventoryItemId' AND i.is_deleted = FALSE
            ) agg ON TRUE;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP VIEW IF EXISTS menu_view;");

        migrationBuilder.Sql("""
            CREATE OR REPLACE VIEW menu_view AS
            SELECT
                m.id,
                m.description,
                m.owner_id,
                COALESCE(r.name, p.name, sh.name) AS owner_name,
                COALESCE(r.image, p.image, sh.image) AS owner_image,
                m.image,
                m.display_currency,
                m.is_active,
                m.category_id::text AS category_id,
                c.name AS category_name,
                m.is_published,
                m.is_scheduled,
                m.published_at,
                (
                    SELECT json_agg(json_build_object(
                        'id', i.id,
                        'name', i.name,
                        'displayPrice', i.display_price,
                        'sortOrder', 0,
                        'isRequired', COALESCE((raw_entry->>'IsRequired')::boolean, false),
                        'variety', (
                            SELECT json_build_object(
                                'id', v.id,
                                'name', v.name,
                                'options', v.varieties
                            )
                            FROM variety v
                            WHERE v.inventory_item_ids @> to_jsonb(i.id)
                            LIMIT 1
                        )
                    ))
                    FROM jsonb_array_elements(m.inventory_item_ids) AS raw_entry
                    JOIN inventory_item i ON i.id = raw_entry->>'InventoryItemId' AND i.is_deleted = FALSE
                ) AS items_json,
                CASE
                    WHEN agg.min_price IS NOT NULL AND agg.max_price IS NOT NULL AND agg.min_price <> agg.max_price
                        THEN agg.min_price::text || ' - ' || agg.max_price::text
                    WHEN agg.min_price IS NOT NULL
                        THEN agg.min_price::text
                    ELSE NULL
                END AS price_range,
                m.created_at,
                m.updated_at,
                m.row_version
            FROM menu m
            LEFT JOIN restaurants r ON r.id = m.owner_id
            LEFT JOIN pharmacies p ON p.id = m.owner_id
            LEFT JOIN shops sh ON sh.id = m.owner_id
            LEFT JOIN category c ON c.id = m.category_id
            LEFT JOIN LATERAL (
                SELECT
                    MIN(i.display_price) AS min_price,
                    MAX(i.display_price) AS max_price
                FROM jsonb_array_elements(m.inventory_item_ids) AS raw_entry
                JOIN inventory_item i ON i.id = raw_entry->>'InventoryItemId' AND i.is_deleted = FALSE
            ) agg ON TRUE;
            """);

        migrationBuilder.DropTable(name: "retailers");
    }
}
