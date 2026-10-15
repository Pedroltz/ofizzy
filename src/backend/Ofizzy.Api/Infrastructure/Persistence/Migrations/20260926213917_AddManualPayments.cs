using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ofizzy.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddManualPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payments",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                    table.UniqueConstraint("AK_payments_TenantId_Id", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_payments_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "ofizzy",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_work_orders_TenantId_WorkOrderId",
                        columns: x => new { x.TenantId, x.WorkOrderId },
                        principalSchema: "ofizzy",
                        principalTable: "work_orders",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment_allocations",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_allocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_allocations_fiscal_documents_TenantId_DocumentId",
                        columns: x => new { x.TenantId, x.DocumentId },
                        principalSchema: "ofizzy",
                        principalTable: "fiscal_documents",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_allocations_payments_TenantId_PaymentId",
                        columns: x => new { x.TenantId, x.PaymentId },
                        principalSchema: "ofizzy",
                        principalTable: "payments",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_allocations_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "ofizzy",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment_movements",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Fees = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SegregatedTax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SettlementId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_movements", x => x.Id);
                    table.UniqueConstraint("AK_payment_movements_TenantId_PaymentId_Id", x => new { x.TenantId, x.PaymentId, x.Id });
                    table.ForeignKey(
                        name: "FK_payment_movements_payment_movements_TenantId_PaymentId_Sett~",
                        columns: x => new { x.TenantId, x.PaymentId, x.SettlementId },
                        principalSchema: "ofizzy",
                        principalTable: "payment_movements",
                        principalColumns: new[] { "TenantId", "PaymentId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_movements_payments_TenantId_PaymentId",
                        columns: x => new { x.TenantId, x.PaymentId },
                        principalSchema: "ofizzy",
                        principalTable: "payments",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_movements_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "ofizzy",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payment_allocations_TenantId_DocumentId",
                schema: "ofizzy",
                table: "payment_allocations",
                columns: new[] { "TenantId", "DocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_payment_allocations_TenantId_PaymentId_DocumentId",
                schema: "ofizzy",
                table: "payment_allocations",
                columns: new[] { "TenantId", "PaymentId", "DocumentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_movements_TenantId_PaymentId_SettlementId",
                schema: "ofizzy",
                table: "payment_movements",
                columns: new[] { "TenantId", "PaymentId", "SettlementId" });

            migrationBuilder.CreateIndex(
                name: "IX_payment_movements_TenantId_RequestId",
                schema: "ofizzy",
                table: "payment_movements",
                columns: new[] { "TenantId", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_TenantId_RequestId",
                schema: "ofizzy",
                table: "payments",
                columns: new[] { "TenantId", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_TenantId_WorkOrderId",
                schema: "ofizzy",
                table: "payments",
                columns: new[] { "TenantId", "WorkOrderId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_allocations",
                schema: "ofizzy");

            migrationBuilder.DropTable(
                name: "payment_movements",
                schema: "ofizzy");

            migrationBuilder.DropTable(
                name: "payments",
                schema: "ofizzy");
        }
    }
}
