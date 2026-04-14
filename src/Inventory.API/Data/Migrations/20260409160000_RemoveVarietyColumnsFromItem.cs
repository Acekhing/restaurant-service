using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVarietyColumnsFromItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "is_variety", table: "inventory_item");
            migrationBuilder.DropColumn(name: "variety_id", table: "inventory_item");
            migrationBuilder.DropColumn(name: "variety_differentiator", table: "inventory_item");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_variety",
                table: "inventory_item",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "variety_id",
                table: "inventory_item",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "variety_differentiator",
                table: "inventory_item",
                type: "text",
                nullable: true);
        }
    }
}
