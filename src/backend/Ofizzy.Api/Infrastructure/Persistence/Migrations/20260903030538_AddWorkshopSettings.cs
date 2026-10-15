using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ofizzy.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Cnpj",
                schema: "ofizzy",
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
                schema: "ofizzy",
                table: "companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "ofizzy",
                table: "companies",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                schema: "ofizzy",
                table: "companies",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                schema: "ofizzy",
                table: "companies",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNotes",
                schema: "ofizzy",
                table: "companies",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                schema: "ofizzy",
                table: "companies",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarrantyTerms",
                schema: "ofizzy",
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
                schema: "ofizzy",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "ofizzy",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "LegalName",
                schema: "ofizzy",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                schema: "ofizzy",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "ReceiptNotes",
                schema: "ofizzy",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "State",
                schema: "ofizzy",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "WarrantyTerms",
                schema: "ofizzy",
                table: "companies");

            migrationBuilder.AlterColumn<string>(
                name: "Cnpj",
                schema: "ofizzy",
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
