using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportPneus.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Cnpj",
                schema: "sport_pneus",
                table: "companies",
                type: "character varying(18)",
                maxLength: 18,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldMaxLength: 14,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "sport_pneus",
                table: "companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "sport_pneus",
                table: "companies",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                schema: "sport_pneus",
                table: "companies",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                schema: "sport_pneus",
                table: "companies",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNotes",
                schema: "sport_pneus",
                table: "companies",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                schema: "sport_pneus",
                table: "companies",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarrantyTerms",
                schema: "sport_pneus",
                table: "companies",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                schema: "sport_pneus",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "sport_pneus",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "LegalName",
                schema: "sport_pneus",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                schema: "sport_pneus",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "ReceiptNotes",
                schema: "sport_pneus",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "State",
                schema: "sport_pneus",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "WarrantyTerms",
                schema: "sport_pneus",
                table: "companies");

            migrationBuilder.AlterColumn<string>(
                name: "Cnpj",
                schema: "sport_pneus",
                table: "companies",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(18)",
                oldMaxLength: 18,
                oldNullable: true);
        }
    }
}
