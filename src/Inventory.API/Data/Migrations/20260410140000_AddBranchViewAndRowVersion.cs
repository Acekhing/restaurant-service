using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchViewAndRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "row_version",
                table: "branches",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.Sql("""
                CREATE OR REPLACE VIEW branch_view AS
                SELECT
                    b.id,
                    b.retailer_id,
                    b.retailer_type,
                    r.business_name AS retailer_business_name,
                    b.business_name,
                    b.business_phone_number,
                    b.business_email,
                    b.account_manager,
                    b.longitude,
                    b.latitude,
                    b.location_name,
                    b.address,
                    b.city,
                    b.zone,
                    b.zone_id,
                    b.main_station,
                    b.main_station_id,
                    b.stations,
                    b.stations_ids,
                    b.payment_methods,
                    b.preferred_payment_methods,
                    b.auto_sweep_account,
                    b.auto_sweep_enabled,
                    b.has_take_payment,
                    b.fineract_account_id,
                    b.fineract_client_id,
                    b.fineract_commission_account_id,
                    b.has_commission_services,
                    b.commission_percentage,
                    b.commission_flat,
                    b.opening_day_hours,
                    b.display_times,
                    b.is_subscribed_to_ready_to_open_notification,
                    b.status,
                    b.is_setup_on_portal,
                    b.is_hidden,
                    b.is_deleted,
                    b.temporary_closed,
                    b.permanently_closed,
                    b.is_ready_to_serve,
                    b.classification,
                    b.is_franchise,
                    b.display_image,
                    b.raw_image_url,
                    b.logo_url,
                    b.color_code,
                    b.restaurant_images,
                    b.search_terms,
                    b.delivery_fee,
                    b.has_rider_payout,
                    b.rider_payout_amount,
                    b.service_markup,
                    b.service_markup_notes,
                    b.has_pharmacist_on_standby,
                    b.has_air_condition,
                    b.restaurant_type,
                    b.meal_types,
                    b.cuisines,
                    b.active_payment_method,
                    b.service_charge,
                    b.created_at,
                    b.updated_at,
                    b.row_version
                FROM branches b
                LEFT JOIN retailers r ON r.id = b.retailer_id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS branch_view;");

            migrationBuilder.DropColumn(
                name: "row_version",
                table: "branches");
        }
    }
}
