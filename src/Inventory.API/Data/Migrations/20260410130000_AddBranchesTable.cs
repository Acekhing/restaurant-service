using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Data.Migrations;

/// <inheritdoc />
public partial class AddBranchesTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "branches",
            columns: table => new
            {
                id = table.Column<string>(type: "text", nullable: false),
                retailer_type = table.Column<string>(type: "text", nullable: false),
                retailer_id = table.Column<string>(type: "text", nullable: false),
                business_name = table.Column<string>(type: "text", nullable: true),
                business_phone_number = table.Column<string>(type: "text", nullable: true),
                business_email = table.Column<string>(type: "text", nullable: true),
                account_manager = table.Column<string>(type: "text", nullable: true),
                longitude = table.Column<double>(type: "double precision", nullable: false),
                latitude = table.Column<double>(type: "double precision", nullable: false),
                location_name = table.Column<string>(type: "text", nullable: true),
                address = table.Column<string>(type: "text", nullable: true),
                city = table.Column<string>(type: "text", nullable: true),
                zone = table.Column<string>(type: "text", nullable: true),
                zone_id = table.Column<string>(type: "text", nullable: true),
                main_station = table.Column<string>(type: "text", nullable: true),
                main_station_id = table.Column<string>(type: "text", nullable: true),
                stations = table.Column<string>(type: "jsonb", nullable: false),
                stations_ids = table.Column<string>(type: "jsonb", nullable: false),
                payment_methods = table.Column<string>(type: "jsonb", nullable: true),
                preferred_payment_methods = table.Column<string>(type: "jsonb", nullable: true),
                auto_sweep_account = table.Column<string>(type: "text", nullable: true),
                auto_sweep_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                has_take_payment = table.Column<bool>(type: "boolean", nullable: false),
                fineract_account_id = table.Column<string>(type: "text", nullable: true),
                fineract_client_id = table.Column<string>(type: "text", nullable: true),
                fineract_commission_account_id = table.Column<string>(type: "text", nullable: true),
                has_commission_services = table.Column<bool>(type: "boolean", nullable: false),
                commission_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                commission_flat = table.Column<decimal>(type: "numeric", nullable: false),
                opening_day_hours = table.Column<string>(type: "jsonb", nullable: false),
                display_times = table.Column<string>(type: "jsonb", nullable: true),
                is_subscribed_to_ready_to_open_notification = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                status = table.Column<string>(type: "text", nullable: true),
                is_setup_on_portal = table.Column<bool>(type: "boolean", nullable: false),
                is_hidden = table.Column<bool>(type: "boolean", nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                temporary_closed = table.Column<bool>(type: "boolean", nullable: false),
                permanently_closed = table.Column<bool>(type: "boolean", nullable: false),
                is_ready_to_serve = table.Column<bool>(type: "boolean", nullable: false),
                classification = table.Column<string>(type: "text", nullable: false, defaultValue: "Day-time Quick Retailer"),
                is_franchise = table.Column<bool>(type: "boolean", nullable: false),
                display_image = table.Column<string>(type: "text", nullable: true),
                raw_image_url = table.Column<string>(type: "text", nullable: true),
                logo_url = table.Column<string>(type: "text", nullable: true),
                color_code = table.Column<string>(type: "text", nullable: true),
                restaurant_images = table.Column<string>(type: "jsonb", nullable: true),
                search_terms = table.Column<string>(type: "jsonb", nullable: true),
                delivery_fee = table.Column<decimal>(type: "numeric", nullable: false),
                has_rider_payout = table.Column<bool>(type: "boolean", nullable: false),
                rider_payout_amount = table.Column<decimal>(type: "numeric", nullable: false),
                service_markup = table.Column<decimal>(type: "numeric", nullable: false),
                service_markup_notes = table.Column<string>(type: "text", nullable: true),
                has_pharmacist_on_standby = table.Column<bool>(type: "boolean", nullable: true),
                has_air_condition = table.Column<bool>(type: "boolean", nullable: true),
                restaurant_type = table.Column<string>(type: "text", nullable: true),
                meal_types = table.Column<string>(type: "jsonb", nullable: true),
                cuisines = table.Column<string[]>(type: "text[]", nullable: true),
                active_payment_method = table.Column<string>(type: "text", nullable: true),
                service_charge = table.Column<decimal>(type: "numeric", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<string>(type: "text", nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                last_updated_by = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_branches", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_branches_retailer_id",
            table: "branches",
            column: "retailer_id");

        migrationBuilder.CreateIndex(
            name: "ix_branches_retailer_type",
            table: "branches",
            column: "retailer_type");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "branches");
    }
}
