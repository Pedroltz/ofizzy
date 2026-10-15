using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ofizzy.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customers",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Document = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WhatsApp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "parts",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CostPrice = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    SalePrice = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "services",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DefaultPrice = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_services", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                schema: "ofizzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Plate = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Brand = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Model = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Year = table.Column<short>(type: "smallint", nullable: true),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Mileage = table.Column<int>(type: "integer", nullable: true),
                    Chassis = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicles_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "ofizzy",
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_customers_Document",
                schema: "ofizzy",
                table: "customers",
                column: "Document",
                unique: true,
                filter: "\"Document\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_customers_Name",
                schema: "ofizzy",
                table: "customers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_customers_Phone",
                schema: "ofizzy",
                table: "customers",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "IX_parts_Code",
                schema: "ofizzy",
                table: "parts",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parts_Name",
                schema: "ofizzy",
                table: "parts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_services_Name",
                schema: "ofizzy",
                table: "services",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_CustomerId",
                schema: "ofizzy",
                table: "vehicles",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_Model",
                schema: "ofizzy",
                table: "vehicles",
                column: "Model");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_Plate",
                schema: "ofizzy",
                table: "vehicles",
                column: "Plate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parts",
                schema: "ofizzy");

            migrationBuilder.DropTable(
                name: "services",
                schema: "ofizzy");

            migrationBuilder.DropTable(
                name: "vehicles",
                schema: "ofizzy");

            migrationBuilder.DropTable(
                name: "customers",
                schema: "ofizzy");
        }
    }
}
