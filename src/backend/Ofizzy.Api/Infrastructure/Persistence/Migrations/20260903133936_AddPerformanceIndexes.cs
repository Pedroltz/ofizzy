using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ofizzy.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_work_orders_CreatedAt",
                schema: "ofizzy",
                table: "work_orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_CustomerName",
                schema: "ofizzy",
                table: "work_orders",
                column: "CustomerName");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_Status",
                schema: "ofizzy",
                table: "work_orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_Status_CreatedAt",
                schema: "ofizzy",
                table: "work_orders",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_VehiclePlate",
                schema: "ofizzy",
                table: "work_orders",
                column: "VehiclePlate");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_IsActive",
                schema: "ofizzy",
                table: "vehicles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_customers_IsActive",
                schema: "ofizzy",
                table: "customers",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_work_orders_CreatedAt",
                schema: "ofizzy",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_work_orders_CustomerName",
                schema: "ofizzy",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_work_orders_Status",
                schema: "ofizzy",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_work_orders_Status_CreatedAt",
                schema: "ofizzy",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_work_orders_VehiclePlate",
                schema: "ofizzy",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_vehicles_IsActive",
                schema: "ofizzy",
                table: "vehicles");

            migrationBuilder.DropIndex(
                name: "IX_customers_IsActive",
                schema: "ofizzy",
                table: "customers");
        }
    }
}
