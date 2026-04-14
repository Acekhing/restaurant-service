using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Data.Migrations;

/// <inheritdoc />
public partial class AddPrepTimeAndTypedOpeningHours : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "average_preparation_time",
            table: "inventory_item",
            type: "integer",
            nullable: true);

        migrationBuilder.Sql("DROP VIEW IF EXISTS inventory_view;");

        migrationBuilder.Sql("""
            ALTER TABLE inventory_availability
            ALTER COLUMN opening_day_hours TYPE jsonb
            USING CASE
                WHEN opening_day_hours IS NOT NULL THEN opening_day_hours::jsonb
                ELSE NULL
            END;
            """);

        migrationBuilder.Sql("""
            CREATE OR REPLACE VIEW inventory_view AS
            SELECT
                i.id,
                i.name,
                i.short_name,
                i.item_type,
                i.tags,
                i.notes,
                i.default_image_url  AS image,
                i.raw_image_url,
                i.is_original_image,
                i.display_price,
                i.supplier_price,
                i.was_display_price  AS old_selling_price,
                i.delivery_fee,
                i.additional_delivery_fee,
                i.additional_delivery_fee_description,
                i.price_with_delivery_fee,
                i.price_range,
                EXISTS(
                    SELECT 1 FROM inventory_item_promotion ip
                    WHERE ip.inventory_item_ids @> to_jsonb(i.id) AND ip.is_active = TRUE
                ) AS has_deals,
                i.display_currency,
                i.average_preparation_time,
                (
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
                    WHERE v.inventory_item_ids @> to_jsonb(i.id)
                    LIMIT 1
                ) AS variety,
                COALESCE(a.is_available, TRUE) AS is_available,
                COALESCE(a.out_of_stock, FALSE) AS out_of_stock,
                a.opening_day_hours,
                a.display_times,
                COALESCE(r.name, p.name, sh.name) AS owner_name,
                COALESCE(r.image, p.image, sh.image) AS owner_image,
                i.owner_id,
                i.station_id,
                i.zone_id
            FROM inventory_item i
            LEFT JOIN inventory_availability a ON a.inventory_item_id = i.id
            LEFT JOIN restaurants r ON r.id = i.owner_id
            LEFT JOIN pharmacies p ON p.id = i.owner_id
            LEFT JOIN shops sh ON sh.id = i.owner_id
            WHERE i.is_deleted = FALSE;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP VIEW IF EXISTS inventory_view;");

        migrationBuilder.Sql("""
            ALTER TABLE inventory_availability
            ALTER COLUMN opening_day_hours TYPE text
            USING opening_day_hours::text;
            """);

        migrationBuilder.DropColumn(
            name: "average_preparation_time",
            table: "inventory_item");

        migrationBuilder.Sql("""
            CREATE OR REPLACE VIEW inventory_view AS
            SELECT
                i.id,
                i.name,
                i.short_name,
                i.item_type,
                i.tags,
                i.notes,
                i.default_image_url  AS image,
                i.raw_image_url,
                i.is_original_image,
                i.display_price,
                i.supplier_price,
                i.was_display_price  AS old_selling_price,
                i.delivery_fee,
                i.additional_delivery_fee,
                i.additional_delivery_fee_description,
                i.price_with_delivery_fee,
                i.price_range,
                EXISTS(
                    SELECT 1 FROM inventory_item_promotion ip
                    WHERE ip.inventory_item_ids @> to_jsonb(i.id) AND ip.is_active = TRUE
                ) AS has_deals,
                i.display_currency,
                (
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
                    WHERE v.inventory_item_ids @> to_jsonb(i.id)
                    LIMIT 1
                ) AS variety,
                COALESCE(a.is_available, TRUE) AS is_available,
                COALESCE(a.out_of_stock, FALSE) AS out_of_stock,
                a.opening_day_hours,
                a.display_times,
                COALESCE(r.name, p.name, sh.name) AS owner_name,
                COALESCE(r.image, p.image, sh.image) AS owner_image,
                i.owner_id,
                i.station_id,
                i.zone_id
            FROM inventory_item i
            LEFT JOIN inventory_availability a ON a.inventory_item_id = i.id
            LEFT JOIN restaurants r ON r.id = i.owner_id
            LEFT JOIN pharmacies p ON p.id = i.owner_id
            LEFT JOIN shops sh ON sh.id = i.owner_id
            WHERE i.is_deleted = FALSE;
            """);
    }
}
