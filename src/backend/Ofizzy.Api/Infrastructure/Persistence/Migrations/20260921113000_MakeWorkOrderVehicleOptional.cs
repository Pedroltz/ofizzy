using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ofizzy.Api.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260921113000_MakeWorkOrderVehicleOptional")]
public partial class MakeWorkOrderVehicleOptional : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<Guid>(
            name: "VehicleId",
            schema: "ofizzy",
            table: "work_orders",
            type: "uuid",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uuid");

        migrationBuilder.AlterColumn<string>(
            name: "VehiclePlate",
            schema: "ofizzy",
            table: "work_orders",
            type: "character varying(8)",
            maxLength: 8,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(8)",
            oldMaxLength: 8);

        migrationBuilder.AlterColumn<string>(
            name: "VehicleDescription",
            schema: "ofizzy",
            table: "work_orders",
            type: "character varying(300)",
            maxLength: 300,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(300)",
            oldMaxLength: 300);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DO $$
            BEGIN
                IF EXISTS (SELECT 1 FROM ofizzy.work_orders WHERE "VehicleId" IS NULL) THEN
                    RAISE EXCEPTION 'Não é possível reverter: existem ordens de serviço sem veículo.';
                END IF;
            END $$;
            """);

        migrationBuilder.Sql(
            "UPDATE ofizzy.work_orders SET \"VehiclePlate\" = COALESCE(\"VehiclePlate\", ''), \"VehicleDescription\" = COALESCE(\"VehicleDescription\", '');");

        migrationBuilder.AlterColumn<Guid>(
            name: "VehicleId",
            schema: "ofizzy",
            table: "work_orders",
            type: "uuid",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "VehiclePlate",
            schema: "ofizzy",
            table: "work_orders",
            type: "character varying(8)",
            maxLength: 8,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "character varying(8)",
            oldMaxLength: 8,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "VehicleDescription",
            schema: "ofizzy",
            table: "work_orders",
            type: "character varying(300)",
            maxLength: 300,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "character varying(300)",
            oldMaxLength: 300,
            oldNullable: true);
    }
}
