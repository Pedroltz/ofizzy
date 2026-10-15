using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ofizzy.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSaasTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vehicles_customers_CustomerId",
                schema: "ofizzy",
                table: "vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_work_order_parts_parts_PartId",
                schema: "ofizzy",
                table: "work_order_parts");

            migrationBuilder.DropForeignKey(
                name: "FK_work_order_parts_work_orders_WorkOrderId",
                schema: "ofizzy",
                table: "work_order_parts");

            migrationBuilder.DropForeignKey(
                name: "FK_work_order_services_services_ServiceId",
                schema: "ofizzy",
                table: "work_order_services");

            migrationBuilder.DropForeignKey(
                name: "FK_work_order_services_work_orders_WorkOrderId",
                schema: "ofizzy",
                table: "work_order_services");

            migrationBuilder.DropForeignKey(
                name: "FK_work_orders_customers_CustomerId",
                schema: "ofizzy",
                table: "work_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_work_orders_vehicles_VehicleId",
                schema: "ofizzy",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_work_orders_CreatedAt",
                schema: "ofizzy",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_work_orders_CustomerId",
                schema: "ofizzy",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_work_orders_CustomerName",
                schema: "ofizzy",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_work_orders_Number",
                schema: "ofizzy",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_work_orders_Status",
                schema: "ofizzy",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_work_orders_Status_CreatedAt",
                schema: "ofizzy",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_work_orders_VehicleId",
                schema: "ofizzy",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_work_orders_VehiclePlate",
                schema: "ofizzy",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_work_order_services_ServiceId",
                schema: "ofizzy",
                table: "work_order_services");

            migrationBuilder.DropIndex(
                name: "IX_work_order_services_WorkOrderId",
                schema: "ofizzy",
                table: "work_order_services");

            migrationBuilder.DropIndex(
                name: "IX_work_order_parts_PartId",
                schema: "ofizzy",
                table: "work_order_parts");

            migrationBuilder.DropIndex(
                name: "IX_work_order_parts_WorkOrderId",
                schema: "ofizzy",
                table: "work_order_parts");

            migrationBuilder.DropIndex(
                name: "IX_vehicles_CustomerId",
                schema: "ofizzy",
                table: "vehicles");

            migrationBuilder.DropIndex(
                name: "IX_vehicles_IsActive",
                schema: "ofizzy",
                table: "vehicles");

            migrationBuilder.DropIndex(
                name: "IX_vehicles_Model",
                schema: "ofizzy",
                table: "vehicles");

            migrationBuilder.DropIndex(
                name: "IX_vehicles_Plate",
                schema: "ofizzy",
                table: "vehicles");

            migrationBuilder.DropIndex(
                name: "IX_services_Name",
                schema: "ofizzy",
                table: "services");

            migrationBuilder.DropIndex(
                name: "IX_parts_Code",
                schema: "ofizzy",
                table: "parts");

            migrationBuilder.DropIndex(
                name: "IX_parts_Name",
                schema: "ofizzy",
                table: "parts");

            migrationBuilder.DropIndex(
                name: "IX_customers_Document",
                schema: "ofizzy",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_customers_IsActive",
                schema: "ofizzy",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_customers_Name",
                schema: "ofizzy",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_customers_Phone",
                schema: "ofizzy",
                table: "customers");

            migrationBuilder.AlterColumn<long>(
                name: "Number",
                schema: "ofizzy",
                table: "work_orders",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValueSql: "nextval('ofizzy.work_order_number_seq')");

            migrationBuilder.DropSequence(
                name: "work_order_number_seq",
                schema: "ofizzy");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "ofizzy",
                table: "work_orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "ofizzy",
                table: "work_order_services",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "ofizzy",
                table: "work_order_parts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "ofizzy",
                table: "vehicles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPlatformAdmin",
                schema: "ofizzy",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PlatformAdminGrantedAt",
                schema: "ofizzy",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "ofizzy",
                table: "services",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "ofizzy",
                table: "refresh_tokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "ofizzy",
                table: "parts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "ofizzy",
                table: "customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "ofizzy",
                table: "companies",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "LastWorkOrderNumber",
                schema: "ofizzy",
                table: "companies",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "ofizzy",
                table: "companies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                schema: "ofizzy",
                table: "companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Vertical = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OnboardingCompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_modules",
                schema: "ofizzy",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Module = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_modules", x => new { x.TenantId, x.Module });
                    table.ForeignKey(
                        name: "FK_tenant_modules_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "ofizzy",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tenant_users",
                schema: "ofizzy",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_users", x => new { x.TenantId, x.UserId });
                    table.ForeignKey(
                        name: "FK_tenant_users_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "ofizzy",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tenant_users_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "ofizzy",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Migration runs transactionally. Never guess ownership in a malformed legacy database.
            migrationBuilder.Sql("""
                DO $migration$
                DECLARE legacy uuid := '01990000-0000-7000-8000-000000000001';
                BEGIN
                    IF (SELECT count(*) FROM ofizzy.companies) > 1 THEN
                        RAISE EXCEPTION 'Multiple legacy companies: ownership requires an explicit migration mapping';
                    END IF;
                    IF EXISTS (SELECT 1 FROM ofizzy.users) OR EXISTS (SELECT 1 FROM ofizzy.companies) THEN
                        INSERT INTO ofizzy.tenants ("Id", "Name", "Slug", "Status", "Vertical", "CreatedAt", "UpdatedAt", "OnboardingCompletedAt")
                        VALUES (legacy, COALESCE((SELECT "Name" FROM ofizzy.companies LIMIT 1), 'Empresa existente'), 'legacy', 'Active', 'Automotive', now(), now(), now());
                        IF NOT EXISTS (SELECT 1 FROM ofizzy.companies) THEN
                            INSERT INTO ofizzy.companies ("Id", "Name", "CreatedAt", "UpdatedAt", "Currency", "Timezone", "LastWorkOrderNumber")
                            VALUES (uuidv7(), 'Empresa existente', now(), now(), 'BRL', 'America/Sao_Paulo', 0);
                        END IF;
                        UPDATE ofizzy.companies SET "TenantId" = legacy, "Currency" = 'BRL', "Timezone" = 'America/Sao_Paulo',
                            "LastWorkOrderNumber" = COALESCE((SELECT max("Number") FROM ofizzy.work_orders), 0);
                        UPDATE ofizzy.customers SET "TenantId" = legacy;
                        UPDATE ofizzy.vehicles SET "TenantId" = legacy;
                        UPDATE ofizzy.services SET "TenantId" = legacy;
                        UPDATE ofizzy.parts SET "TenantId" = legacy;
                        UPDATE ofizzy.work_orders SET "TenantId" = legacy;
                        UPDATE ofizzy.work_order_services SET "TenantId" = legacy;
                        UPDATE ofizzy.work_order_parts SET "TenantId" = legacy;
                        UPDATE ofizzy.refresh_tokens SET "TenantId" = legacy;
                        INSERT INTO ofizzy.tenant_users ("TenantId", "UserId", "Role", "IsActive", "CreatedAt", "UpdatedAt")
                        SELECT legacy, "Id", 'Owner', "IsActive", "CreatedAt", now() FROM ofizzy.users;
                        INSERT INTO ofizzy.tenant_modules ("TenantId", "Module", "Enabled")
                        SELECT legacy, module, true FROM unnest(ARRAY['Customers','WorkOrders','Catalog','Automotive']) AS module;
                    END IF;
                END $migration$;
                """);
            foreach (var table in new[] { "companies", "customers", "vehicles", "services", "parts", "work_orders", "work_order_services", "work_order_parts" })
                migrationBuilder.AlterColumn<Guid>(name: "TenantId", schema: "ofizzy", table: table, type: "uuid", nullable: false, oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_work_orders_TenantId_Id",
                schema: "ofizzy",
                table: "work_orders",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_vehicles_TenantId_Id",
                schema: "ofizzy",
                table: "vehicles",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_services_TenantId_Id",
                schema: "ofizzy",
                table: "services",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_parts_TenantId_Id",
                schema: "ofizzy",
                table: "parts",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_customers_TenantId_Id",
                schema: "ofizzy",
                table: "customers",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_TenantId_CreatedAt",
                schema: "ofizzy",
                table: "work_orders",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_TenantId_CustomerId",
                schema: "ofizzy",
                table: "work_orders",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_TenantId_CustomerName",
                schema: "ofizzy",
                table: "work_orders",
                columns: new[] { "TenantId", "CustomerName" });

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_TenantId_Number",
                schema: "ofizzy",
                table: "work_orders",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_TenantId_Status",
                schema: "ofizzy",
                table: "work_orders",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_TenantId_Status_CreatedAt",
                schema: "ofizzy",
                table: "work_orders",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_TenantId_VehicleId",
                schema: "ofizzy",
                table: "work_orders",
                columns: new[] { "TenantId", "VehicleId" });

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_TenantId_VehiclePlate",
                schema: "ofizzy",
                table: "work_orders",
                columns: new[] { "TenantId", "VehiclePlate" });

            migrationBuilder.CreateIndex(
                name: "IX_work_order_services_TenantId_ServiceId",
                schema: "ofizzy",
                table: "work_order_services",
                columns: new[] { "TenantId", "ServiceId" });

            migrationBuilder.CreateIndex(
                name: "IX_work_order_services_TenantId_WorkOrderId",
                schema: "ofizzy",
                table: "work_order_services",
                columns: new[] { "TenantId", "WorkOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_work_order_parts_TenantId_PartId",
                schema: "ofizzy",
                table: "work_order_parts",
                columns: new[] { "TenantId", "PartId" });

            migrationBuilder.CreateIndex(
                name: "IX_work_order_parts_TenantId_WorkOrderId",
                schema: "ofizzy",
                table: "work_order_parts",
                columns: new[] { "TenantId", "WorkOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_TenantId_CustomerId",
                schema: "ofizzy",
                table: "vehicles",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_TenantId_IsActive",
                schema: "ofizzy",
                table: "vehicles",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_TenantId_Model",
                schema: "ofizzy",
                table: "vehicles",
                columns: new[] { "TenantId", "Model" });

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_TenantId_Plate",
                schema: "ofizzy",
                table: "vehicles",
                columns: new[] { "TenantId", "Plate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_services_TenantId_Name",
                schema: "ofizzy",
                table: "services",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_parts_TenantId_Code",
                schema: "ofizzy",
                table: "parts",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parts_TenantId_Name",
                schema: "ofizzy",
                table: "parts",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_customers_TenantId_Document",
                schema: "ofizzy",
                table: "customers",
                columns: new[] { "TenantId", "Document" },
                unique: true,
                filter: "\"Document\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_customers_TenantId_IsActive",
                schema: "ofizzy",
                table: "customers",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_customers_TenantId_Name",
                schema: "ofizzy",
                table: "customers",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_customers_TenantId_Phone",
                schema: "ofizzy",
                table: "customers",
                columns: new[] { "TenantId", "Phone" });

            migrationBuilder.CreateIndex(
                name: "IX_companies_TenantId",
                schema: "ofizzy",
                table: "companies",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_users_UserId",
                schema: "ofizzy",
                table: "tenant_users",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_Slug",
                schema: "ofizzy",
                table: "tenants",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_companies_tenants_TenantId",
                schema: "ofizzy",
                table: "companies",
                column: "TenantId",
                principalSchema: "ofizzy",
                principalTable: "tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_customers_tenants_TenantId",
                schema: "ofizzy",
                table: "customers",
                column: "TenantId",
                principalSchema: "ofizzy",
                principalTable: "tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_parts_tenants_TenantId",
                schema: "ofizzy",
                table: "parts",
                column: "TenantId",
                principalSchema: "ofizzy",
                principalTable: "tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_services_tenants_TenantId",
                schema: "ofizzy",
                table: "services",
                column: "TenantId",
                principalSchema: "ofizzy",
                principalTable: "tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicles_customers_TenantId_CustomerId",
                schema: "ofizzy",
                table: "vehicles",
                columns: new[] { "TenantId", "CustomerId" },
                principalSchema: "ofizzy",
                principalTable: "customers",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicles_tenants_TenantId",
                schema: "ofizzy",
                table: "vehicles",
                column: "TenantId",
                principalSchema: "ofizzy",
                principalTable: "tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_work_order_parts_parts_TenantId_PartId",
                schema: "ofizzy",
                table: "work_order_parts",
                columns: new[] { "TenantId", "PartId" },
                principalSchema: "ofizzy",
                principalTable: "parts",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_work_order_parts_tenants_TenantId",
                schema: "ofizzy",
                table: "work_order_parts",
                column: "TenantId",
                principalSchema: "ofizzy",
                principalTable: "tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_work_order_parts_work_orders_TenantId_WorkOrderId",
                schema: "ofizzy",
                table: "work_order_parts",
                columns: new[] { "TenantId", "WorkOrderId" },
                principalSchema: "ofizzy",
                principalTable: "work_orders",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_work_order_services_services_TenantId_ServiceId",
                schema: "ofizzy",
                table: "work_order_services",
                columns: new[] { "TenantId", "ServiceId" },
                principalSchema: "ofizzy",
                principalTable: "services",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_work_order_services_tenants_TenantId",
                schema: "ofizzy",
                table: "work_order_services",
                column: "TenantId",
                principalSchema: "ofizzy",
                principalTable: "tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_work_order_services_work_orders_TenantId_WorkOrderId",
                schema: "ofizzy",
                table: "work_order_services",
                columns: new[] { "TenantId", "WorkOrderId" },
                principalSchema: "ofizzy",
                principalTable: "work_orders",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_work_orders_customers_TenantId_CustomerId",
                schema: "ofizzy",
                table: "work_orders",
                columns: new[] { "TenantId", "CustomerId" },
                principalSchema: "ofizzy",
                principalTable: "customers",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_work_orders_tenants_TenantId",
                schema: "ofizzy",
                table: "work_orders",
                column: "TenantId",
                principalSchema: "ofizzy",
                principalTable: "tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_work_orders_vehicles_TenantId_VehicleId",
                schema: "ofizzy",
                table: "work_orders",
                columns: new[] { "TenantId", "VehicleId" },
                principalSchema: "ofizzy",
                principalTable: "vehicles",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Restore a verified pre-tenancy backup; a downgrade would destroy tenant isolation.");
        }
    }
}
