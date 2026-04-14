using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePromotionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_inventory_item_promotion_inventory_item_inventory_item_id",
                table: "inventory_item_promotion");

            migrationBuilder.DropColumn(
                name: "discount_amount",
                table: "inventory_item_promotion");

            migrationBuilder.RenameColumn(
                name: "inventory_item_id",
                table: "inventory_item_promotion",
                newName: "owner_id");

            migrationBuilder.RenameIndex(
                name: "ix_inventory_item_promotion_inventory_item_id",
                table: "inventory_item_promotion",
                newName: "ix_inventory_item_promotion_owner_id");

            migrationBuilder.AddColumn<int>(
                name: "discount_in_percentage",
                table: "inventory_item_promotion",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "inventory_item_ids",
                table: "inventory_item_promotion",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_applied_to_items",
                table: "inventory_item_promotion",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_applied_to_menu",
                table: "inventory_item_promotion",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_free_delivery",
                table: "inventory_item_promotion",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "discount_in_percentage",
                table: "inventory_item_promotion");

            migrationBuilder.DropColumn(
                name: "inventory_item_ids",
                table: "inventory_item_promotion");

            migrationBuilder.DropColumn(
                name: "is_applied_to_items",
                table: "inventory_item_promotion");

            migrationBuilder.DropColumn(
                name: "is_applied_to_menu",
                table: "inventory_item_promotion");

            migrationBuilder.DropColumn(
                name: "is_free_delivery",
                table: "inventory_item_promotion");

            migrationBuilder.RenameColumn(
                name: "owner_id",
                table: "inventory_item_promotion",
                newName: "inventory_item_id");

            migrationBuilder.RenameIndex(
                name: "ix_inventory_item_promotion_owner_id",
                table: "inventory_item_promotion",
                newName: "ix_inventory_item_promotion_inventory_item_id");

            migrationBuilder.AddColumn<decimal>(
                name: "discount_amount",
                table: "inventory_item_promotion",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_item_promotion_inventory_item_inventory_item_id",
                table: "inventory_item_promotion",
                column: "inventory_item_id",
                principalTable: "inventory_item",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
