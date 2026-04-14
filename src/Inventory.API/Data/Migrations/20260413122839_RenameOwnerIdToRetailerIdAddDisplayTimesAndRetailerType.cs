using System.Collections.Generic;
using Inventory.Contracts.ReadModel;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameOwnerIdToRetailerIdAddDisplayTimesAndRetailerType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS inventory_view;");

            migrationBuilder.DropIndex(
                name: "ix_inventory_item_owner_id",
                table: "inventory_item");

            migrationBuilder.RenameColumn(
                name: "owner_id",
                table: "inventory_item",
                newName: "retailer_id");

            migrationBuilder.AddColumn<string>(
                name: "retailer_type",
                table: "inventory_item",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<InventoryOpeningHours>>(
                name: "display_times",
                table: "inventory_item",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_inventory_item_retailer_id",
                table: "inventory_item",
                column: "retailer_id");

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
                    i.display_times,
                    i.retailer_id,
                    i.retailer_type,
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

            migrationBuilder.DropIndex(
                name: "ix_inventory_item_retailer_id",
                table: "inventory_item");

            migrationBuilder.DropColumn(
                name: "display_times",
                table: "inventory_item");

            migrationBuilder.DropColumn(
                name: "retailer_type",
                table: "inventory_item");

            migrationBuilder.RenameColumn(
                name: "retailer_id",
                table: "inventory_item",
                newName: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_item_owner_id",
                table: "inventory_item",
                column: "owner_id");

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
    }
}
