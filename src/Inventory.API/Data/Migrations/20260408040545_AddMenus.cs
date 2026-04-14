using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMenus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "menu",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    owner_id = table.Column<string>(type: "text", nullable: false),
                    item_type = table.Column<string>(type: "text", nullable: false),
                    image = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_menu", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "menu_item",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    menu_id = table.Column<string>(type: "text", nullable: false),
                    inventory_item_id = table.Column<string>(type: "text", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    added_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                        name: "fk_menu_stats_menus_menu_id",
                        column: x => x.menu_id,
                        principalTable: "menu",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_menu_item_type",
                table: "menu",
                column: "item_type");

            migrationBuilder.CreateIndex(
                name: "ix_menu_owner_id",
                table: "menu",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_menu_item_inventory_item_id",
                table: "menu_item",
                column: "inventory_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_menu_item_menu_id",
                table: "menu_item",
                column: "menu_id");

            migrationBuilder.CreateIndex(
                name: "ix_menu_stats_menu_id",
                table: "menu_stats",
                column: "menu_id",
                unique: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS menu_view;");

            migrationBuilder.DropTable(
                name: "menu_item");

            migrationBuilder.DropTable(
                name: "menu_stats");

            migrationBuilder.DropTable(
                name: "menu");
        }
    }
}
