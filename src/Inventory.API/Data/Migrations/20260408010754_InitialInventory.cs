using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inventory_item",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    short_name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    summary_description = table.Column<string>(type: "text", nullable: true),
                    default_image_url = table.Column<string>(type: "text", nullable: true),
                    raw_image_url = table.Column<string>(type: "text", nullable: true),
                    is_original_image = table.Column<bool>(type: "boolean", nullable: false),
                    item_type = table.Column<string>(type: "text", nullable: false),
                    tags = table.Column<string>(type: "text", nullable: true),
                    search_tags = table.Column<string>(type: "text", nullable: true),
                    search_tags_auto_generated = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    owner_id = table.Column<string>(type: "text", nullable: false),
                    station_id = table.Column<string>(type: "text", nullable: true),
                    zone_id = table.Column<string>(type: "text", nullable: true),
                    has_variety = table.Column<bool>(type: "boolean", nullable: false),
                    is_variety = table.Column<bool>(type: "boolean", nullable: false),
                    variety_id = table.Column<string>(type: "text", nullable: true),
                    variety_differentiator = table.Column<string>(type: "text", nullable: true),
                    attributes = table.Column<string>(type: "text", nullable: true),
                    images = table.Column<string>(type: "text", nullable: true),
                    display_price = table.Column<decimal>(type: "numeric", nullable: false),
                    was_display_price = table.Column<decimal>(type: "numeric", nullable: true),
                    supplier_price = table.Column<decimal>(type: "numeric", nullable: true),
                    delivery_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    additional_delivery_fee = table.Column<decimal>(type: "numeric", nullable: true),
                    additional_delivery_fee_description = table.Column<string>(type: "text", nullable: true),
                    price_with_delivery_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    display_currency = table.Column<string>(type: "text", nullable: false),
                    price_range = table.Column<string>(type: "text", nullable: true),
                    promotion = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("pk_inventory_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_outbox",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    aggregate_id = table.Column<string>(type: "text", nullable: false),
                    aggregate_type = table.Column<string>(type: "text", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    actor_id = table.Column<string>(type: "text", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_outbox", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pharmacies",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    image = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pharmacies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "restaurants",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    image = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restaurants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shops",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    image = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shops", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_availability",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    inventory_item_id = table.Column<string>(type: "text", nullable: false),
                    is_available = table.Column<bool>(type: "boolean", nullable: false),
                    out_of_stock = table.Column<bool>(type: "boolean", nullable: false),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false),
                    stock_quantity = table.Column<int>(type: "integer", nullable: true),
                    schedule_ahead = table.Column<bool>(type: "boolean", nullable: false),
                    opening_day_hours = table.Column<string>(type: "text", nullable: true),
                    display_times = table.Column<string>(type: "text", nullable: true),
                    out_of_stock_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    out_of_stock_updated_by = table.Column<string>(type: "text", nullable: true),
                    row_version = table.Column<long>(type: "bigint", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_availability", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_availability_inventory_item_inventory_item_id",
                        column: x => x.inventory_item_id,
                        principalTable: "inventory_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inventory_item_promotion",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    inventory_item_id = table.Column<string>(type: "text", nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
                    effective_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    effective_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_item_promotion", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_item_promotion_inventory_item_inventory_item_id",
                        column: x => x.inventory_item_id,
                        principalTable: "inventory_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                        name: "fk_inventory_stats_inventory_items_inventory_item_id",
                        column: x => x.inventory_item_id,
                        principalTable: "inventory_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_availability_inventory_item_id",
                table: "inventory_availability",
                column: "inventory_item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inventory_item_item_type",
                table: "inventory_item",
                column: "item_type");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_item_owner_id",
                table: "inventory_item",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_item_promotion_inventory_item_id",
                table: "inventory_item_promotion",
                column: "inventory_item_id");

            migrationBuilder.CreateIndex(
                name: "idx_outbox_unprocessed",
                table: "inventory_outbox",
                column: "occurred_at",
                filter: "processed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_stats_inventory_item_id",
                table: "inventory_stats",
                column: "inventory_item_id",
                unique: true);

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
                    (i.promotion IS NOT NULL) AS has_deals,
                    i.display_currency,
                    COALESCE(a.is_available, TRUE) AS is_available,
                    COALESCE(a.out_of_stock, FALSE) AS out_of_stock,
                    a.opening_day_hours,
                    a.display_times,
                    COALESCE(s.view_count, 0) AS view_count,
                    COALESCE(s.sold_count, 0) AS total_orders,
                    COALESCE(s.average_rating, 0) AS average_rating,
                    COALESCE(s.total_ratings_count, 0) AS total_ratings_count,
                    COALESCE(r.name, p.name, sh.name) AS owner_name,
                    COALESCE(r.image, p.image, sh.image) AS owner_image,
                    i.owner_id,
                    i.station_id,
                    i.zone_id
                FROM inventory_item i
                LEFT JOIN inventory_availability a ON a.inventory_item_id = i.id
                LEFT JOIN inventory_stats s ON s.inventory_item_id = i.id
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

            migrationBuilder.DropTable(
                name: "inventory_availability");

            migrationBuilder.DropTable(
                name: "inventory_item_promotion");

            migrationBuilder.DropTable(
                name: "inventory_outbox");

            migrationBuilder.DropTable(
                name: "inventory_stats");

            migrationBuilder.DropTable(
                name: "pharmacies");

            migrationBuilder.DropTable(
                name: "restaurants");

            migrationBuilder.DropTable(
                name: "shops");

            migrationBuilder.DropTable(
                name: "inventory_item");
        }
    }
}
