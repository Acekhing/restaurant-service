using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRequiredToMenuItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert existing inventory_item_ids from ["id1","id2"]
            // to [{"InventoryItemId":"id1","IsRequired":false},...]
            migrationBuilder.Sql("""
                UPDATE menu
                SET inventory_item_ids = (
                    SELECT jsonb_agg(jsonb_build_object('InventoryItemId', elem, 'IsRequired', false))
                    FROM jsonb_array_elements_text(inventory_item_ids) AS elem
                )
                WHERE inventory_item_ids IS NOT NULL;
                """);

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
                            'isRequired', COALESCE((raw_entry->>'IsRequired')::boolean, false)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert inventory_item_ids back to simple string arrays
            migrationBuilder.Sql("""
                UPDATE menu
                SET inventory_item_ids = (
                    SELECT jsonb_agg(elem->>'InventoryItemId')
                    FROM jsonb_array_elements(inventory_item_ids) AS elem
                )
                WHERE inventory_item_ids IS NOT NULL;
                """);

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
