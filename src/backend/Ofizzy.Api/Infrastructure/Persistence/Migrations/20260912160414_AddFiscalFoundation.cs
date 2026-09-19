using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ofizzy.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFiscalFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fiscal_documents",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Environment = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    Series = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<long>(type: "bigint", nullable: false),
                    Identity = table.Column<string>(type: "text", nullable: false),
                    AccessKey = table.Column<string>(type: "text", nullable: true),
                    Protocol = table.Column<string>(type: "text", nullable: true),
                    Receipt = table.Column<string>(type: "text", nullable: true),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Snapshot = table.Column<string>(type: "text", nullable: false),
                    SubmittedXml = table.Column<string>(type: "text", nullable: true),
                    AuthorizedXml = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LeaseUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiscal_documents", x => x.Id);
                    table.UniqueConstraint("AK_fiscal_documents_TenantId_Id", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_fiscal_documents_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "ofizzy",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fiscal_documents_work_orders_TenantId_WorkOrderId",
                        columns: x => new { x.TenantId, x.WorkOrderId },
                        principalSchema: "ofizzy",
                        principalTable: "work_orders",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fiscal_preparations",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiscal_preparations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fiscal_preparations_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "ofizzy",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fiscal_preparations_work_orders_TenantId_WorkOrderId",
                        columns: x => new { x.TenantId, x.WorkOrderId },
                        principalSchema: "ofizzy",
                        principalTable: "work_orders",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fiscal_products",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiscal_products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fiscal_products_parts_TenantId_PartId",
                        columns: x => new { x.TenantId, x.PartId },
                        principalSchema: "ofizzy",
                        principalTable: "parts",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fiscal_products_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "ofizzy",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fiscal_sequences",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Environment = table.Column<int>(type: "integer", nullable: false),
                    Series = table.Column<int>(type: "integer", nullable: false),
                    LastNumber = table.Column<long>(type: "bigint", nullable: false),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiscal_sequences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fiscal_sequences_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "ofizzy",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fiscal_services",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiscal_services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fiscal_services_services_TenantId_ServiceId",
                        columns: x => new { x.TenantId, x.ServiceId },
                        principalSchema: "ofizzy",
                        principalTable: "services",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fiscal_services_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "ofizzy",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fiscal_settings",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false),
                    Certificate = table.Column<byte[]>(type: "bytea", nullable: true),
                    KeyId = table.Column<string>(type: "text", nullable: true),
                    CertificateSubject = table.Column<string>(type: "text", nullable: true),
                    CertificateThumbprint = table.Column<string>(type: "text", nullable: true),
                    CertificateExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiscal_settings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fiscal_settings_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "ofizzy",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fiscal_events",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Operation = table.Column<string>(type: "text", nullable: false),
                    RequestXml = table.Column<string>(type: "text", nullable: true),
                    ResponseXml = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiscal_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fiscal_events_fiscal_documents_TenantId_DocumentId",
                        columns: x => new { x.TenantId, x.DocumentId },
                        principalSchema: "ofizzy",
                        principalTable: "fiscal_documents",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fiscal_events_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "ofizzy",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_documents_TenantId_Kind_Environment_Series_Number",
                schema: "ofizzy",
                table: "fiscal_documents",
                columns: new[] { "TenantId", "Kind", "Environment", "Series", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_documents_TenantId_WorkOrderId_Kind_Environment",
                schema: "ofizzy",
                table: "fiscal_documents",
                columns: new[] { "TenantId", "WorkOrderId", "Kind", "Environment" },
                unique: true,
                filter: "\"State\" <> 7");

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_events_TenantId_DocumentId",
                schema: "ofizzy",
                table: "fiscal_events",
                columns: new[] { "TenantId", "DocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_preparations_TenantId_WorkOrderId",
                schema: "ofizzy",
                table: "fiscal_preparations",
                columns: new[] { "TenantId", "WorkOrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_products_TenantId_PartId",
                schema: "ofizzy",
                table: "fiscal_products",
                columns: new[] { "TenantId", "PartId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_sequences_TenantId_Kind_Environment_Series",
                schema: "ofizzy",
                table: "fiscal_sequences",
                columns: new[] { "TenantId", "Kind", "Environment", "Series" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_services_TenantId_ServiceId",
                schema: "ofizzy",
                table: "fiscal_services",
                columns: new[] { "TenantId", "ServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_settings_TenantId",
                schema: "ofizzy",
                table: "fiscal_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fiscal_events",
                schema: "ofizzy");

            migrationBuilder.DropTable(
                name: "fiscal_preparations",
                schema: "ofizzy");

            migrationBuilder.DropTable(
                name: "fiscal_products",
                schema: "ofizzy");

            migrationBuilder.DropTable(
                name: "fiscal_sequences",
                schema: "ofizzy");

            migrationBuilder.DropTable(
                name: "fiscal_services",
                schema: "ofizzy");

            migrationBuilder.DropTable(
                name: "fiscal_settings",
                schema: "ofizzy");

            migrationBuilder.DropTable(
                name: "fiscal_documents",
                schema: "ofizzy");
        }
    }
}
