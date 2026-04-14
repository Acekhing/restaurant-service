using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuPricingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "delivery_fee",
                table: "menu",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "display_currency",
                table: "menu",
                type: "text",
                nullable: false,
                defaultValue: "GHS");

            migrationBuilder.Sql("DROP VIEW IF EXISTS menu_view;");

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
                    COALESCE(s.view_count, 0) AS view_count,
                    COALESCE(s.search_count, 0) AS search_count,
                    COALESCE(s.cart_count, 0) AS cart_count,
                    COALESCE(s.sold_count, 0) AS sold_count,
                    COALESCE(s.number_shared, 0) AS number_shared,
                    COALESCE(s.average_rating, 0) AS average_rating,
                    COALESCE(s.total_ratings_count, 0) AS total_ratings_count,
                    s.average_rating_description,
                    m.created_at,
                    m.updated_at,
                    m.row_version
                FROM menu m
                LEFT JOIN menu_stats s ON s.menu_id = m.id
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
            migrationBuilder.Sql("DROP VIEW IF EXISTS menu_view;");

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
                    COALESCE(s.view_count, 0) AS view_count,
                    COALESCE(s.search_count, 0) AS search_count,
                    COALESCE(s.cart_count, 0) AS cart_count,
                    COALESCE(s.sold_count, 0) AS sold_count,
                    COALESCE(s.number_shared, 0) AS number_shared,
                    COALESCE(s.average_rating, 0) AS average_rating,
                    COALESCE(s.total_ratings_count, 0) AS total_ratings_count,
                    s.average_rating_description,
                    m.created_at,
                    m.updated_at,
                    m.row_version
                FROM menu m
                LEFT JOIN menu_stats s ON s.menu_id = m.id
                LEFT JOIN restaurants r ON r.id = m.owner_id
                LEFT JOIN pharmacies p ON p.id = m.owner_id
                LEFT JOIN shops sh ON sh.id = m.owner_id
                WHERE m.is_deleted = FALSE;
                """);

            migrationBuilder.DropColumn(
                name: "delivery_fee",
                table: "menu");

            migrationBuilder.DropColumn(
                name: "display_currency",
                table: "menu");
        }
    }
}
