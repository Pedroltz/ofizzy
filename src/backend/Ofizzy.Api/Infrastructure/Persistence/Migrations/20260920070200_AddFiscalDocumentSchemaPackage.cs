using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ofizzy.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFiscalDocumentSchemaPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SchemaPackage",
                schema: "ofizzy",
                table: "fiscal_documents",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "Legado sem pacote identificado");

            migrationBuilder.Sql("""
                UPDATE ofizzy.fiscal_documents
                SET "SchemaPackage" = CASE "Kind"
                    WHEN 0 THEN 'NF-e PL_010c'
                    WHEN 1 THEN 'NFS-e Nacional 1.01'
                    ELSE 'Legado sem pacote identificado'
                END
                WHERE "SchemaPackage" = 'Legado sem pacote identificado';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SchemaPackage",
                schema: "ofizzy",
                table: "fiscal_documents");
        }
    }
}
