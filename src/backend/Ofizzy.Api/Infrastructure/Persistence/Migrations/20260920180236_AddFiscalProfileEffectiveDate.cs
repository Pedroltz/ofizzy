using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ofizzy.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFiscalProfileEffectiveDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_fiscal_services_TenantId_ServiceId",
                schema: "ofizzy",
                table: "fiscal_services");

            migrationBuilder.DropIndex(
                name: "IX_fiscal_products_TenantId_PartId",
                schema: "ofizzy",
                table: "fiscal_products");

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveFrom",
                schema: "ofizzy",
                table: "fiscal_services",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(2000, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveFrom",
                schema: "ofizzy",
                table: "fiscal_products",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(2000, 1, 1));

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_services_TenantId_ServiceId_EffectiveFrom",
                schema: "ofizzy",
                table: "fiscal_services",
                columns: new[] { "TenantId", "ServiceId", "EffectiveFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_products_TenantId_PartId_EffectiveFrom",
                schema: "ofizzy",
                table: "fiscal_products",
                columns: new[] { "TenantId", "PartId", "EffectiveFrom" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_fiscal_services_TenantId_ServiceId_EffectiveFrom",
                schema: "ofizzy",
                table: "fiscal_services");

            migrationBuilder.DropIndex(
                name: "IX_fiscal_products_TenantId_PartId_EffectiveFrom",
                schema: "ofizzy",
                table: "fiscal_products");

            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                schema: "ofizzy",
                table: "fiscal_services");

            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                schema: "ofizzy",
                table: "fiscal_products");

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_services_TenantId_ServiceId",
                schema: "ofizzy",
                table: "fiscal_services",
                columns: new[] { "TenantId", "ServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_products_TenantId_PartId",
                schema: "ofizzy",
                table: "fiscal_products",
                columns: new[] { "TenantId", "PartId" },
                unique: true);
        }
    }
}
