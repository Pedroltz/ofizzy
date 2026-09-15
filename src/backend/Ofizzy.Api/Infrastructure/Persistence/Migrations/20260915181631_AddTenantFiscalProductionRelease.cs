using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ofizzy.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantFiscalProductionRelease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FiscalProductionReleased",
                schema: "ofizzy",
                table: "tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FiscalProductionReleasedAt",
                schema: "ofizzy",
                table: "tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FiscalProductionReleasedByUserId",
                schema: "ofizzy",
                table: "tenants",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FiscalProductionReleased",
                schema: "ofizzy",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "FiscalProductionReleasedAt",
                schema: "ofizzy",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "FiscalProductionReleasedByUserId",
                schema: "ofizzy",
                table: "tenants");
        }
    }
}
