using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Ofizzy.Api.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Ofizzy.IntegrationTests;

public sealed class TechnicalIdentifierMigrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("ofizzy_migration_tests")
        .WithUsername("tests")
        .WithPassword("tests-password")
        .Build();

    [Fact]
    public async Task Migration_renames_existing_application_schema_without_recreating_data()
    {
        await using (var connection = new NpgsqlConnection(_postgres.GetConnectionString()))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText =
                """
                CREATE TABLE "__EFMigrationsHistory" (
                    "MigrationId" character varying(150) NOT NULL PRIMARY KEY,
                    "ProductVersion" character varying(32) NOT NULL
                );

                INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES
                    ('20260901024647_InitialIdentity', '10.0.11'),
                    ('20260901030801_AddCatalogs', '10.0.11'),
                    ('20260902233650_AddWorkOrders', '10.0.11'),
                    ('20260903030538_AddWorkshopSettings', '10.0.11'),
                    ('20260903133936_AddPerformanceIndexes', '10.0.11');

                CREATE SCHEMA prebrand;
                CREATE TABLE prebrand.companies (id integer);
                CREATE TABLE prebrand.users (id integer);
                CREATE TABLE prebrand.customers (id integer);
                CREATE TABLE prebrand.vehicles (id integer);
                CREATE TABLE prebrand.work_orders (id integer);
                INSERT INTO prebrand.companies VALUES (1);
                """;
            await command.ExecuteNonQueryAsync();
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        await using (var context = new ApplicationDbContext(options))
        {
            await context.GetService<IMigrator>().MigrateAsync("20260908120000_RenameTechnicalIdentifiersToOfizzy");
        }

        await using var verificationConnection = new NpgsqlConnection(_postgres.GetConnectionString());
        await verificationConnection.OpenAsync();
        await using var verificationCommand = verificationConnection.CreateCommand();
        verificationCommand.CommandText =
            """
            SELECT
                EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'ofizzy'),
                EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'prebrand'),
                (SELECT COUNT(*) FROM ofizzy.companies),
                EXISTS (
                    SELECT 1 FROM "__EFMigrationsHistory"
                    WHERE "MigrationId" = '20260908120000_RenameTechnicalIdentifiersToOfizzy'
                );
            """;
        await using var reader = await verificationCommand.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.True(reader.GetBoolean(0));
        Assert.False(reader.GetBoolean(1));
        Assert.Equal(1, reader.GetInt64(2));
        Assert.True(reader.GetBoolean(3));
    }

    public Task InitializeAsync() => _postgres.StartAsync();

    public async Task DisposeAsync() => await _postgres.DisposeAsync();
}
