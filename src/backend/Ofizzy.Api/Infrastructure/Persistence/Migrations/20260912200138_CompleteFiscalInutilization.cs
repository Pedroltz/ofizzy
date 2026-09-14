using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ofizzy.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteFiscalInutilization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_fiscal_documents_TenantId_WorkOrderId_Kind_Environment",
                schema: "ofizzy",
                table: "fiscal_documents");

            migrationBuilder.CreateTable(
                name: "fiscal_inutilizations",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Environment = table.Column<int>(type: "integer", nullable: false),
                    Series = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    FirstNumber = table.Column<long>(type: "bigint", nullable: false),
                    LastNumber = table.Column<long>(type: "bigint", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    RequestXml = table.Column<string>(type: "text", nullable: true),
                    ResponseXml = table.Column<string>(type: "text", nullable: true),
                    Protocol = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiscal_inutilizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fiscal_inutilizations_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "ofizzy",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_documents_TenantId_WorkOrderId_Kind_Environment",
                schema: "ofizzy",
                table: "fiscal_documents",
                columns: new[] { "TenantId", "WorkOrderId", "Kind", "Environment" },
                unique: true,
                filter: "\"State\" NOT IN (6, 7)");

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_inutilizations_TenantId_Environment_Series_Year_Firs~",
                schema: "ofizzy",
                table: "fiscal_inutilizations",
                columns: new[] { "TenantId", "Environment", "Series", "Year", "FirstNumber", "LastNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fiscal_inutilizations",
                schema: "ofizzy");

            migrationBuilder.DropIndex(
                name: "IX_fiscal_documents_TenantId_WorkOrderId_Kind_Environment",
                schema: "ofizzy",
                table: "fiscal_documents");

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_documents_TenantId_WorkOrderId_Kind_Environment",
                schema: "ofizzy",
                table: "fiscal_documents",
                columns: new[] { "TenantId", "WorkOrderId", "Kind", "Environment" },
                unique: true,
                filter: "\"State\" <> 7");
        }
    }
}
