using System.Net.Http.Json;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ofizzy.Api.Infrastructure.Persistence;

namespace Ofizzy.IntegrationTests;
public sealed class TenantAuthenticationTests(OfizzyFactory factory) : IClassFixture<OfizzyFactory>
{
    [Fact]
    public async Task Multiple_tenants_require_selection_and_refresh_preserves_it()
    {
        using var client = factory.CreateClient(new() { HandleCookies = false });
        var session = new TenantTestSession(client); await session.Bootstrap();
        var first = await session.Provision("Alpha", "alpha", "owner@example.test", "Oficina2026");
        var second = await session.Provision("Beta", "beta", "owner@example.test");
        // A failed provisioning does not reset an existing password or leave partial rows.
        var failed = await session.Send(HttpMethod.Post, "/api/platform/tenants", new { name = "Failed", slug = "failed", vertical = "Automotive", adminName = "Owner", email = "owner@example.test", password = "AnotherPass123", modules = new[] { "Customers" } });
        Assert.Equal(HttpStatusCode.Conflict, failed.StatusCode);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(2, await db.Tenants.CountAsync()); Assert.Equal(2, await db.TenantUsers.CountAsync());
            Assert.Equal(2, await db.Users.CountAsync());
        }
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.ExecuteSqlRawAsync("""
                CREATE FUNCTION ofizzy.fail_provision_for_test() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN IF EXISTS (SELECT 1 FROM ofizzy.tenants WHERE "Id" = NEW."TenantId" AND "Slug" = 'rollback') THEN
                    RAISE EXCEPTION 'Injected transaction failure'; END IF; RETURN NEW; END $$;
                CREATE TRIGGER fail_provision BEFORE INSERT ON ofizzy.tenant_users FOR EACH ROW EXECUTE FUNCTION ofizzy.fail_provision_for_test();
                """);
        }
        var rollback = await session.Send(HttpMethod.Post, "/api/platform/tenants", new { name = "Rollback", slug = "rollback", vertical = "Automotive", adminName = "Owner", email = "rollback@example.test", password = "AnotherPass123" });
        Assert.Equal(HttpStatusCode.InternalServerError, rollback.StatusCode);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(2, await db.Tenants.CountAsync()); Assert.Equal(2, await db.Users.CountAsync());
            Assert.Equal(2, await db.Database.SqlQueryRaw<long>("SELECT count(*) AS \"Value\" FROM ofizzy.companies").SingleAsync());
        }
        using var ownerClient = factory.CreateClient(new() { HandleCookies = false }); var owner = new TenantTestSession(ownerClient); await owner.Login("owner@example.test");
        var me = await owner.Json(HttpMethod.Get, "/api/auth/me"); Assert.Equal(System.Text.Json.JsonValueKind.Null, me.GetProperty("tenant").ValueKind);
        Assert.Equal(HttpStatusCode.Forbidden, (await owner.Send(HttpMethod.Get, "/api/customers")).StatusCode);
        Assert.Equal(2, (await owner.Json(HttpMethod.Get, "/api/auth/tenants")).GetArrayLength());
        var firstId = first.GetProperty("id").GetGuid(); var secondId = second.GetProperty("id").GetGuid();
        await owner.Select(firstId, true);
        var refreshed = await owner.Json(HttpMethod.Post, "/api/auth/refresh", new { });
        Assert.Equal(firstId, refreshed.GetProperty("tenant").GetProperty("id").GetGuid());
        await owner.Select(secondId, true);
        Assert.Equal(secondId, (await owner.Json(HttpMethod.Post, "/api/auth/refresh", new { })).GetProperty("tenant").GetProperty("id").GetGuid());
        Assert.Equal(HttpStatusCode.Forbidden, (await owner.Send(HttpMethod.Post, "/api/platform/tenants", new { })).StatusCode);
        // Capture a refresh token, rotate, then replay: the replacement family must be revoked.
        var oldCookies = ownerClient.DefaultRequestHeaders.GetValues("Cookie").Single();
        await owner.Json(HttpMethod.Post, "/api/auth/refresh", new { });
        var currentCookies = ownerClient.DefaultRequestHeaders.GetValues("Cookie").Single();
        ownerClient.DefaultRequestHeaders.Remove("Cookie"); ownerClient.DefaultRequestHeaders.Add("Cookie", oldCookies);
        Assert.Equal(HttpStatusCode.Unauthorized, (await ownerClient.PostAsJsonAsync("/api/auth/refresh", new { })).StatusCode);
        ownerClient.DefaultRequestHeaders.Remove("Cookie"); ownerClient.DefaultRequestHeaders.Add("Cookie", currentCookies);
        Assert.Equal(HttpStatusCode.Unauthorized, (await ownerClient.PostAsJsonAsync("/api/auth/refresh", new { })).StatusCode);
    }
}
