using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ofizzy.Api.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260908120000_RenameTechnicalIdentifiersToOfizzy")]
public sealed class RenameTechnicalIdentifiersToOfizzy : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DO $ofizzy$
            DECLARE
                source_schema text;
                candidate_count integer;
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM information_schema.schemata
                    WHERE schema_name = 'ofizzy'
                ) THEN
                    RETURN;
                END IF;

                SELECT COUNT(*), MIN(schema_name)
                INTO candidate_count, source_schema
                FROM (
                    SELECT table_schema AS schema_name
                    FROM information_schema.tables
                    WHERE table_schema NOT IN ('information_schema', 'pg_catalog', 'public')
                      AND table_name IN ('companies', 'users', 'customers', 'vehicles', 'work_orders')
                    GROUP BY table_schema
                    HAVING COUNT(DISTINCT table_name) = 5
                ) AS candidates;

                IF candidate_count = 0 THEN
                    RAISE EXCEPTION 'Não foi possível localizar o schema existente do Ofizzy.';
                END IF;

                IF candidate_count > 1 THEN
                    RAISE EXCEPTION 'Mais de um schema candidato foi localizado; migração cancelada por segurança.';
                END IF;

                EXECUTE format('ALTER SCHEMA %I RENAME TO ofizzy', source_schema);
            END
            $ofizzy$;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "A renomeação do schema é irreversível por migration. Restaure o backup operacional para rollback.");
    }
}
