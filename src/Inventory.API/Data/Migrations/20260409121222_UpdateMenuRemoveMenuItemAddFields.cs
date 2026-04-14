using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMenuRemoveMenuItemAddFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS menu_view;");

            migrationBuilder.DropTable(
                name: "menu_item");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "menu");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "menu",
                newName: "is_scheduled");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "menu",
                newName: "published_at");

            migrationBuilder.AddColumn<Guid>(
                name: "category_id",
                table: "menu",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "inventory_item_ids",
                table: "menu",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_published",
                table: "menu",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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
                    m.category_id::text AS category_id,
                    c.name AS category_name,
                    m.is_published,
                    m.is_scheduled,
                    m.published_at,
                    COALESCE(agg.item_count, 0) AS item_count,
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
                LEFT JOIN category c ON c.id = m.category_id
                LEFT JOIN LATERAL (
                    SELECT
                        COUNT(*)::int AS item_count,
                        SUM(i.display_price) AS total_display_price,
                        SUM(COALESCE(i.supplier_price, 0)) AS total_supplier_price,
                        SUM(i.delivery_fee) AS total_delivery_fee,
                        SUM(i.price_with_delivery_fee) AS total_price_with_delivery_fee,
                        SUM(COALESCE(i.additional_delivery_fee, 0)) AS total_additional_delivery_fee,
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

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "menu");

            migrationBuilder.DropColumn(
                name: "inventory_item_ids",
                table: "menu");

            migrationBuilder.DropColumn(
                name: "is_published",
                table: "menu");

            migrationBuilder.RenameColumn(
                name: "published_at",
                table: "menu",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "is_scheduled",
                table: "menu",
                newName: "is_deleted");

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                table: "menu",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "menu_item",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    inventory_item_id = table.Column<string>(type: "text", nullable: false),
                    menu_id = table.Column<string>(type: "text", nullable: false),
                    added_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_menu_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_menu_item_inventory_item_inventory_item_id",
                        column: x => x.inventory_item_id,
                        principalTable: "inventory_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_menu_item_menu_menu_id",
                        column: x => x.menu_id,
                        principalTable: "menu",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_menu_item_inventory_item_id",
                table: "menu_item",
                column: "inventory_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_menu_item_menu_id",
                table: "menu_item",
                column: "menu_id");
        }
    }
}
