using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMenuItemType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS menu_view;");

            migrationBuilder.DropIndex(
                name: "ix_menu_item_type",
                table: "menu");

            migrationBuilder.DropColumn(
                name: "item_type",
                table: "menu");

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
                            'sortOrder', 0
                        ))
                        FROM inventory_item i
                        WHERE i.id IN (SELECT jsonb_array_elements_text(m.inventory_item_ids))
                          AND i.is_deleted = FALSE
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
                    FROM inventory_item i
                    WHERE i.id IN (SELECT jsonb_array_elements_text(m.inventory_item_ids))
                      AND i.is_deleted = FALSE
                ) agg ON TRUE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "item_type",
                table: "menu",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_menu_item_type",
                table: "menu",
                column: "item_type");

            migrationBuilder.Sql("DROP VIEW IF EXISTS menu_view;");

            migrationBuilder.Sql("""
                CREATE OR REPLACE VIEW menu_view AS
                SELECT
                    m.id,
                    m.description,
                    m.owner_id,
                    COALESCE(r.name, p.name, sh.name) AS owner_name,
                    COALESCE(r.image, p.image, sh.image) AS owner_image,
                    m.item_type,
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
                            'sortOrder', 0
                        ))
                        FROM inventory_item i
                        WHERE i.id IN (SELECT jsonb_array_elements_text(m.inventory_item_ids))
                          AND i.is_deleted = FALSE
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
                    FROM inventory_item i
                    WHERE i.id IN (SELECT jsonb_array_elements_text(m.inventory_item_ids))
                      AND i.is_deleted = FALSE
                ) agg ON TRUE;
                """);
        }
    }
}
