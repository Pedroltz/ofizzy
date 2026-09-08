using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ofizzy.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<long>(name: "work_order_number_seq", schema: "ofizzy");

            migrationBuilder.CreateTable(
                name: "work_orders",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('ofizzy.work_order_number_seq')"),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    CustomerDocument = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    CustomerPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VehiclePlate = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    VehicleDescription = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Mileage = table.Column<int>(type: "integer", nullable: true),
                    Complaint = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    Diagnosis = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_work_orders_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "ofizzy",
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_work_orders_vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalSchema: "ofizzy",
                        principalTable: "vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "work_order_parts",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_order_parts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_work_order_parts_parts_PartId",
                        column: x => x.PartId,
                        principalSchema: "ofizzy",
                        principalTable: "parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_work_order_parts_work_orders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "ofizzy",
                        principalTable: "work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "work_order_services",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_order_services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_work_order_services_services_ServiceId",
                        column: x => x.ServiceId,
                        principalSchema: "ofizzy",
                        principalTable: "services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_work_order_services_work_orders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "ofizzy",
                        principalTable: "work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_work_order_parts_PartId",
                schema: "ofizzy",
                table: "work_order_parts",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_parts_WorkOrderId",
                schema: "ofizzy",
                table: "work_order_parts",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_services_ServiceId",
                schema: "ofizzy",
                table: "work_order_services",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_services_WorkOrderId",
                schema: "ofizzy",
                table: "work_order_services",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_CustomerId",
                schema: "ofizzy",
                table: "work_orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_Number",
                schema: "ofizzy",
                table: "work_orders",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_VehicleId",
                schema: "ofizzy",
                table: "work_orders",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "work_order_parts",
                schema: "ofizzy");

            migrationBuilder.DropTable(
                name: "work_order_services",
                schema: "ofizzy");

            migrationBuilder.DropTable(
                name: "work_orders",
                schema: "ofizzy");

            migrationBuilder.DropSequence(name: "work_order_number_seq", schema: "ofizzy");
        }
    }
}
