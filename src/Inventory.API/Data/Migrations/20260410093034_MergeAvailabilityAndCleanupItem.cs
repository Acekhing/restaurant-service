using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class MergeAvailabilityAndCleanupItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS inventory_view;");

            migrationBuilder.AddColumn<bool>(name: "is_available", table: "inventory_item", type: "boolean", nullable: false, defaultValue: true);
            migrationBuilder.AddColumn<bool>(name: "out_of_stock", table: "inventory_item", type: "boolean", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "is_hidden", table: "inventory_item", type: "boolean", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "schedule_ahead", table: "inventory_item", type: "boolean", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<string>(name: "opening_day_hours", table: "inventory_item", type: "jsonb", nullable: true);
            migrationBuilder.AddColumn<bool>(name: "has_deals", table: "inventory_item", type: "boolean", nullable: false, defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE inventory_item i
                SET
                    is_available    = COALESCE(a.is_available, TRUE),
                    out_of_stock    = COALESCE(a.out_of_stock, FALSE),
                    is_hidden       = COALESCE(a.is_hidden, FALSE),
                    schedule_ahead  = COALESCE(a.schedule_ahead, FALSE),
                    opening_day_hours = a.opening_day_hours
                FROM inventory_availability a
                WHERE a.inventory_item_id = i.id;
                """);

            migrationBuilder.Sql("""
                UPDATE inventory_item i
                SET has_deals = TRUE
                WHERE EXISTS (
                    SELECT 1 FROM inventory_item_promotion ip
                    WHERE ip.inventory_item_ids @> to_jsonb(i.id)
                      AND ip.is_active = TRUE
                );
                """);

            migrationBuilder.DropTable(name: "inventory_availability");

            migrationBuilder.DropColumn(name: "additional_delivery_fee", table: "inventory_item");
            migrationBuilder.DropColumn(name: "additional_delivery_fee_description", table: "inventory_item");
            migrationBuilder.DropColumn(name: "price_with_delivery_fee", table: "inventory_item");

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
                    i.price_range,
                    i.has_deals,
                    i.display_currency,
                    i.average_preparation_time,
                    i.has_variety,
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
                    i.is_available,
                    i.out_of_stock,
                    i.opening_day_hours,
                    i.owner_id,
                    i.station_id,
                    i.zone_id
                FROM inventory_item i
                WHERE i.is_deleted = FALSE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS inventory_view;");

            migrationBuilder.AddColumn<decimal>(name: "additional_delivery_fee", table: "inventory_item", type: "numeric", nullable: true);
            migrationBuilder.AddColumn<string>(name: "additional_delivery_fee_description", table: "inventory_item", type: "text", nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "price_with_delivery_fee", table: "inventory_item", type: "numeric", nullable: false, defaultValue: 0m);

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
                    opening_day_hours = table.Column<string>(type: "jsonb", nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "ix_inventory_availability_inventory_item_id",
                table: "inventory_availability",
                column: "inventory_item_id",
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO inventory_availability (id, inventory_item_id, is_available, out_of_stock, is_hidden, stock_quantity, schedule_ahead, opening_day_hours, row_version, updated_at)
                SELECT gen_random_uuid()::text, id, is_available, out_of_stock, is_hidden, NULL, schedule_ahead, opening_day_hours, 0, updated_at
                FROM inventory_item;
                """);

            migrationBuilder.DropColumn(name: "is_available", table: "inventory_item");
            migrationBuilder.DropColumn(name: "out_of_stock", table: "inventory_item");
            migrationBuilder.DropColumn(name: "is_hidden", table: "inventory_item");
            migrationBuilder.DropColumn(name: "schedule_ahead", table: "inventory_item");
            migrationBuilder.DropColumn(name: "opening_day_hours", table: "inventory_item");
            migrationBuilder.DropColumn(name: "has_deals", table: "inventory_item");

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
                    i.has_variety,
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
                    i.owner_id,
                    i.station_id,
                    i.zone_id
                FROM inventory_item i
                LEFT JOIN inventory_availability a ON a.inventory_item_id = i.id
                WHERE i.is_deleted = FALSE;
                """);
        }
    }
}
