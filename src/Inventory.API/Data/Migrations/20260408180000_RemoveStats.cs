using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Data.Migrations;

/// <inheritdoc />
public partial class RemoveStats : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP VIEW IF EXISTS inventory_view;");
        migrationBuilder.Sql("DROP VIEW IF EXISTS menu_view;");

        migrationBuilder.DropTable(name: "inventory_stats");
        migrationBuilder.DropTable(name: "menu_stats");

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
                    WHERE ip.inventory_item_id = i.id AND ip.is_active = TRUE
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

        migrationBuilder.Sql("""
            CREATE OR REPLACE VIEW menu_view AS
            SELECT
                m.id,
                m.name,
                m.description,
                m.owner_id,
                COALESCE(r.name, p.name, sh.name) AS owner_name,
                COALESCE(r.image, p.image, sh.image) AS owner_image,
                m.item_type,
                m.image,
                m.delivery_fee,
                m.display_currency,
                m.is_active,
                (SELECT COUNT(*)::int FROM menu_item mi2 WHERE mi2.menu_id = m.id) AS item_count,
                (
                    SELECT json_agg(json_build_object(
                        'id', i.id,
                        'name', i.name,
                        'image', i.default_image_url,
                        'displayPrice', i.display_price,
                        'displayCurrency', i.display_currency,
                        'isAvailable', COALESCE(a.is_available, TRUE),
                        'sortOrder', mi.sort_order
                    ) ORDER BY mi.sort_order)
                    FROM menu_item mi
                    INNER JOIN inventory_item i ON i.id = mi.inventory_item_id
                    LEFT JOIN inventory_availability a ON a.inventory_item_id = i.id
                    WHERE mi.menu_id = m.id
                ) AS items_json,
                COALESCE(agg.total_display_price, 0) AS total_display_price,
                COALESCE(agg.total_supplier_price, 0) AS total_supplier_price,
                COALESCE(agg.total_delivery_fee, 0) AS total_delivery_fee,
                COALESCE(agg.total_price_with_delivery_fee, 0) AS total_price_with_delivery_fee,
                COALESCE(agg.total_additional_delivery_fee, 0) AS total_additional_delivery_fee,
                COALESCE(agg.total_display_price, 0) - COALESCE(agg.total_supplier_price, 0) AS commission,
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
            LEFT JOIN LATERAL (
                SELECT
                    SUM(i.display_price) AS total_display_price,
                    SUM(COALESCE(i.supplier_price, 0)) AS total_supplier_price,
                    SUM(i.delivery_fee) AS total_delivery_fee,
                    SUM(i.price_with_delivery_fee) AS total_price_with_delivery_fee,
                    SUM(COALESCE(i.additional_delivery_fee, 0)) AS total_additional_delivery_fee,
                    MIN(i.display_price) AS min_price,
                    MAX(i.display_price) AS max_price
                FROM menu_item mi
                INNER JOIN inventory_item i ON i.id = mi.inventory_item_id
                WHERE mi.menu_id = m.id
            ) agg ON TRUE
            WHERE m.is_deleted = FALSE;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "inventory_stats",
            columns: table => new
            {
                id = table.Column<string>(type: "text", nullable: false),
                inventory_item_id = table.Column<string>(type: "text", nullable: false),
                view_count = table.Column<int>(type: "integer", nullable: false),
                search_count = table.Column<int>(type: "integer", nullable: false),
                cart_count = table.Column<int>(type: "integer", nullable: false),
                sold_count = table.Column<int>(type: "integer", nullable: false),
                number_shared = table.Column<int>(type: "integer", nullable: false),
                average_rating = table.Column<double>(type: "double precision", nullable: false),
                total_ratings_count = table.Column<int>(type: "integer", nullable: false),
                average_rating_description = table.Column<string>(type: "text", nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_inventory_stats", x => x.id);
                table.ForeignKey(
                    name: "fk_inventory_stats_inventory_item_inventory_item_id",
                    column: x => x.inventory_item_id,
                    principalTable: "inventory_item",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_inventory_stats_inventory_item_id",
            table: "inventory_stats",
            column: "inventory_item_id",
            unique: true);

        migrationBuilder.CreateTable(
            name: "menu_stats",
            columns: table => new
            {
                id = table.Column<string>(type: "text", nullable: false),
                menu_id = table.Column<string>(type: "text", nullable: false),
                view_count = table.Column<int>(type: "integer", nullable: false),
                search_count = table.Column<int>(type: "integer", nullable: false),
                cart_count = table.Column<int>(type: "integer", nullable: false),
                sold_count = table.Column<int>(type: "integer", nullable: false),
                number_shared = table.Column<int>(type: "integer", nullable: false),
                average_rating = table.Column<double>(type: "double precision", nullable: false),
                total_ratings_count = table.Column<int>(type: "integer", nullable: false),
                average_rating_description = table.Column<string>(type: "text", nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_menu_stats", x => x.id);
                table.ForeignKey(
                    name: "fk_menu_stats_menu_menu_id",
                    column: x => x.menu_id,
                    principalTable: "menu",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_menu_stats_menu_id",
            table: "menu_stats",
            column: "menu_id",
            unique: true);
    }
}
