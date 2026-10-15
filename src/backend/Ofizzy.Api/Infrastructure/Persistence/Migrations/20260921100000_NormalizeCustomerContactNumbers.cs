using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ofizzy.Api.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260921100000_NormalizeCustomerContactNumbers")]
public partial class NormalizeCustomerContactNumbers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE ofizzy.customers
            SET "Phone" = NULLIF(regexp_replace(COALESCE("Phone", ''), '[^0-9]', '', 'g'), ''),
                "WhatsApp" = NULLIF(regexp_replace(COALESCE("WhatsApp", ''), '[^0-9]', '', 'g'), '')
            WHERE "Phone" ~ '[^0-9]' OR "WhatsApp" ~ '[^0-9]';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Contact numbers are canonicalized as digits; display formatting is applied by the UI.
    }
}
