using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ofizzy.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerFiscalAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                schema: "ofizzy",
                table: "customers",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                schema: "ofizzy",
                table: "customers",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Number",
                schema: "ofizzy",
                table: "customers",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                schema: "ofizzy",
                table: "customers",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "ofizzy",
                table: "customers",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                schema: "ofizzy",
                table: "customers",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CityCode",
                schema: "ofizzy",
                table: "customers",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StateRegistration",
                schema: "ofizzy",
                table: "customers",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PostalCode", schema: "ofizzy", table: "customers");
            migrationBuilder.DropColumn(name: "Street", schema: "ofizzy", table: "customers");
            migrationBuilder.DropColumn(name: "Number", schema: "ofizzy", table: "customers");
            migrationBuilder.DropColumn(name: "District", schema: "ofizzy", table: "customers");
            migrationBuilder.DropColumn(name: "City", schema: "ofizzy", table: "customers");
            migrationBuilder.DropColumn(name: "State", schema: "ofizzy", table: "customers");
            migrationBuilder.DropColumn(name: "CityCode", schema: "ofizzy", table: "customers");
            migrationBuilder.DropColumn(name: "StateRegistration", schema: "ofizzy", table: "customers");
        }
    }
}
