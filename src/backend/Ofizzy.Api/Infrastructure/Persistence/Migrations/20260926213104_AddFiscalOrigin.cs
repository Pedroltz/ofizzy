using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ofizzy.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFiscalOrigin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Origin",
                schema: "ofizzy",
                table: "fiscal_inutilizations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Origin",
                schema: "ofizzy",
                table: "fiscal_documents",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            // Only positive simulator markers establish provenance. Everything else stays Unknown.
            migrationBuilder.Sql("""
                UPDATE ofizzy.fiscal_documents SET "Origin" = 1
                WHERE "Environment" = 2 AND (
                    "AuthorizedXml" LIKE '%Ofizzy_SIMULACAO%'
                    OR "AuthorizedXml" LIKE '%Simulação de Desenvolvimento%'
                    OR "Message" LIKE '%simulada localmente;%'
                );
                UPDATE ofizzy.fiscal_inutilizations AS i SET "Origin" = 1
                WHERE i."Environment" = 2 AND EXISTS (
                    SELECT 1 FROM ofizzy.fiscal_documents AS d
                    WHERE d."TenantId" = i."TenantId" AND d."Kind" = 0
                    AND d."Environment" = i."Environment" AND d."Series" = i."Series"
                    AND d."Number" BETWEEN i."FirstNumber" AND i."LastNumber"
                ) AND NOT EXISTS (
                    SELECT 1 FROM ofizzy.fiscal_documents AS d
                    WHERE d."TenantId" = i."TenantId" AND d."Kind" = 0
                    AND d."Environment" = i."Environment" AND d."Series" = i."Series"
                    AND d."Number" BETWEEN i."FirstNumber" AND i."LastNumber" AND d."Origin" <> 1
                );
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Origin",
                schema: "ofizzy",
                table: "fiscal_inutilizations");

            migrationBuilder.DropColumn(
                name: "Origin",
                schema: "ofizzy",
                table: "fiscal_documents");
        }
    }
}
