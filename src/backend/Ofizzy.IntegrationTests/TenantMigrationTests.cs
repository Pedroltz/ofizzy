using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Ofizzy.Api.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Ofizzy.IntegrationTests;

public sealed class TenantMigrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:18-alpine").WithDatabase("legacy_tenancy_test").Build();
    public Task InitializeAsync() => postgres.StartAsync();
    public async Task DisposeAsync() => await postgres.DisposeAsync();
    [Fact]
    public async Task Legacy_records_snapshots_ids_and_numbers_are_preserved_and_composite_FKs_enforce_ownership()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
        await using var db = new ApplicationDbContext(options);
        await db.GetService<IMigrator>().MigrateAsync("20260908120000_RenameTechnicalIdentifiersToOfizzy");
        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO ofizzy.companies ("Id","Name","CreatedAt","UpdatedAt") VALUES ('00000000-0000-0000-0000-000000000001','Legacy workshop',now(),now());
            INSERT INTO ofizzy.users ("Id","Name","Email","NormalizedEmail","PasswordHash","IsActive","CreatedAt","UpdatedAt") VALUES ('00000000-0000-0000-0000-000000000002','Owner','test@example.test','TEST@EXAMPLE.TEST','unchanged-hash',true,now(),now());
            INSERT INTO ofizzy.customers ("Id","Name","Document","IsActive","CreatedAt","UpdatedAt") VALUES ('00000000-0000-0000-0000-000000000003','Legacy customer','12345678909',true,now(),now());
            INSERT INTO ofizzy.vehicles ("Id","CustomerId","Plate","Model","IsActive","CreatedAt","UpdatedAt") VALUES ('00000000-0000-0000-0000-000000000004','00000000-0000-0000-0000-000000000003','ABC1D23','Legacy model',true,now(),now());
            INSERT INTO ofizzy.services ("Id","Name","DefaultPrice","IsActive","CreatedAt","UpdatedAt") VALUES ('00000000-0000-0000-0000-000000000005','Legacy service',100,true,now(),now());
            INSERT INTO ofizzy.parts ("Id","Name","Code","CostPrice","SalePrice","IsActive","CreatedAt","UpdatedAt") VALUES ('00000000-0000-0000-0000-000000000006','Legacy part','P1',10,20,true,now(),now());
            INSERT INTO ofizzy.work_orders ("Id","Number","CustomerId","VehicleId","CustomerName","VehiclePlate","VehicleDescription","Status","CreatedAt","UpdatedAt") VALUES ('00000000-0000-0000-0000-000000000007',42,'00000000-0000-0000-0000-000000000003','00000000-0000-0000-0000-000000000004','Historical snapshot','ABC1D23','Historical vehicle','Completed',now(),now());
            INSERT INTO ofizzy.work_order_services ("Id","WorkOrderId","ServiceId","Description","Quantity","UnitPrice") VALUES ('00000000-0000-0000-0000-000000000008','00000000-0000-0000-0000-000000000007','00000000-0000-0000-0000-000000000005','Historical service',2,100);
            INSERT INTO ofizzy.work_order_parts ("Id","WorkOrderId","PartId","Description","Quantity","UnitPrice") VALUES ('00000000-0000-0000-0000-000000000009','00000000-0000-0000-0000-000000000007','00000000-0000-0000-0000-000000000006','Historical part',1,20);
            """);
        await db.Database.MigrateAsync(); await db.Database.MigrateAsync();
        Assert.False(db.Database.HasPendingModelChanges());
        Assert.Equal("unchanged-hash", (await db.Users.SingleAsync()).PasswordHash);
        Assert.False((await db.Users.SingleAsync()).IsPlatformAdmin);
        var tenant = await db.Tenants.SingleAsync(); Assert.Equal("Legacy workshop", tenant.Name);
        Assert.Equal(Ofizzy.Api.Modules.Tenancy.TenantRole.Owner, (await db.TenantUsers.SingleAsync()).Role);
        // Raw SQL is restricted to this migration verification, not exposed to request handlers.
        Assert.Equal(42, await db.Database.SqlQueryRaw<long>("SELECT \"LastWorkOrderNumber\" AS \"Value\" FROM ofizzy.companies").SingleAsync());
        Assert.Equal("Historical snapshot", await db.Database.SqlQueryRaw<string>("SELECT \"CustomerName\" AS \"Value\" FROM ofizzy.work_orders WHERE \"Id\" = '00000000-0000-0000-0000-000000000007'").SingleAsync());
#pragma warning disable EF1002 // Table identifiers are a fixed test-only allowlist, never input.
        foreach (var table in new[] { "companies", "customers", "vehicles", "services", "parts", "work_orders", "work_order_services", "work_order_parts" })
            Assert.Equal(tenant.Id, await db.Database.SqlQueryRaw<Guid>($"SELECT \"TenantId\" AS \"Value\" FROM ofizzy.{table}").SingleAsync());
#pragma warning restore EF1002
        var other = new Ofizzy.Api.Modules.Tenancy.Tenant { Name = "Other", Slug = "other" }; db.Tenants.Add(other); await db.SaveChangesAsync();
        var error = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE ofizzy.vehicles SET \"TenantId\" = {other.Id}"));
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, error.SqlState);
        Assert.Empty(await db.WorkOrders.ToListAsync());
    }
}
