using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Data.Migrations;

/// <inheritdoc />
public partial class AddOrdersTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "orders",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                order_number = table.Column<string>(type: "text", nullable: false),
                retailer_id = table.Column<string>(type: "text", nullable: false),
                waiter_name = table.Column<string>(type: "text", nullable: true),
                table_number = table.Column<string>(type: "text", nullable: true),
                customer_notes = table.Column<string>(type: "text", nullable: true),
                status = table.Column<string>(type: "text", nullable: false),
                total_amount = table.Column<decimal>(type: "numeric", nullable: false),
                display_currency = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_orders", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "order_lines",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                order_id = table.Column<Guid>(type: "uuid", nullable: false),
                inventory_item_id = table.Column<string>(type: "text", nullable: false),
                item_name = table.Column<string>(type: "text", nullable: false),
                unit_price = table.Column<decimal>(type: "numeric", nullable: false),
                quantity = table.Column<int>(type: "integer", nullable: false),
                notes = table.Column<string>(type: "text", nullable: true),
                variety_selection = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_order_lines", x => x.id);
                table.ForeignKey(
                    name: "fk_order_lines_orders_order_id",
                    column: x => x.order_id,
                    principalTable: "orders",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_orders_retailer_id",
            table: "orders",
            column: "retailer_id");

        migrationBuilder.CreateIndex(
            name: "ix_orders_status",
            table: "orders",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "ix_order_lines_order_id",
            table: "order_lines",
            column: "order_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "order_lines");
        migrationBuilder.DropTable(name: "orders");
    }
}
