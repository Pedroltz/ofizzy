using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Modules.Tenancy;

namespace Ofizzy.IntegrationTests;

public sealed class TenantIsolationTests(OfizzyFactory factory) : IClassFixture<OfizzyFactory>
{
    [Fact]
    public async Task Alpha_and_Beta_are_isolated_and_have_independent_order_numbers()
    {
        using var adminClient = factory.CreateClient(new() { HandleCookies = false });
        var admin = new TenantTestSession(adminClient); await admin.Bootstrap();
        var alpha = await admin.Provision("Mecânica Alpha", "alpha", "alpha@example.test", "Oficina2026");
        var beta = await admin.Provision("Mecânica Beta", "beta", "beta@example.test", "Oficina2026");
        var alphaId = alpha.GetProperty("id").GetGuid(); var betaId = beta.GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.Forbidden, (await admin.Send(HttpMethod.Get, "/api/customers")).StatusCode);
        using var aClient = factory.CreateClient(new() { HandleCookies = false }); using var bClient = factory.CreateClient(new() { HandleCookies = false });
        var a = new TenantTestSession(aClient); var b = new TenantTestSession(bClient);
        await a.Login("alpha@example.test"); await b.Login("beta@example.test");
        var me = await a.Json(HttpMethod.Get, "/api/auth/me"); Assert.Equal(alphaId, me.GetProperty("tenant").GetProperty("id").GetGuid());
        Assert.False(me.GetProperty("isPlatformAdmin").GetBoolean());
        Assert.Equal(HttpStatusCode.Forbidden, (await a.Send(HttpMethod.Get, "/api/platform/tenants")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await a.Send(HttpMethod.Post, "/api/auth/tenant", new { tenantId = betaId })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await a.Send(HttpMethod.Get, "/api/customers")).StatusCode);
        await a.Select(alphaId, true); await b.Select(betaId, true);
        var ar = await Seed(a, "João", "ABC1D23"); var br = await Seed(b, "Maria", "XYZ9Z99");
        Assert.Equal(1, ar.Order.GetProperty("number").GetInt64()); Assert.Equal(1, br.Order.GetProperty("number").GetInt64());
        await CheckIsolation(a, br); await CheckIsolation(b, ar);
        Assert.Equal("Mecânica Alpha", (await a.Json(HttpMethod.Get, "/api/company")).GetProperty("name").GetString());
        Assert.Equal("Mecânica Beta", (await b.Json(HttpMethod.Get, "/api/company")).GetProperty("name").GetString());
        Assert.Equal(HttpStatusCode.Conflict, (await a.Send(HttpMethod.Post, "/api/vehicles", Vehicle(br.Customer, "ZZZ1A23"))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await a.Send(HttpMethod.Post, "/api/work-orders", Order(ar.Customer, br.Vehicle))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await a.Send(HttpMethod.Post, "/api/work-orders", Order(ar.Customer, ar.Vehicle, br.Service))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await a.Send(HttpMethod.Post, "/api/customers", Customer("Duplicado"))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await a.Send(HttpMethod.Post, "/api/parts", Part())).StatusCode);
        // Same plate is legal in a different tenant; fixture plates above remain distinct.
        Assert.Equal(HttpStatusCode.Created, (await b.Send(HttpMethod.Post, "/api/vehicles", Vehicle(br.Customer, "ABC1D23"))).StatusCode);
        var concurrent = await Task.WhenAll(Enumerable.Range(0, 6).Select(_ => aClient.PostAsJsonAsync("/api/work-orders", Order(ar.Customer, ar.Vehicle))));
        var numbers = new List<long>();
        foreach (var response in concurrent) { Assert.Equal(HttpStatusCode.Created, response.StatusCode); numbers.Add((await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("number").GetInt64()); }
        Assert.Equal(6, numbers.Distinct().Count()); Assert.Equal(Enumerable.Range(2, 6).Select(x => (long)x), numbers.Order());
        // Membership and status are authoritative even while an access JWT remains valid.
        await admin.Json(HttpMethod.Put, $"/api/platform/tenants/{alphaId}", new { status = "Suspended", modules = new[] { "Customers", "WorkOrders", "Catalog", "Automotive" } });
        Assert.Equal(HttpStatusCode.Forbidden, (await a.Send(HttpMethod.Get, "/api/customers")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await a.Send(HttpMethod.Post, "/api/auth/refresh", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await b.Send(HttpMethod.Get, "/api/customers")).StatusCode);
        await admin.Json(HttpMethod.Put, $"/api/platform/tenants/{betaId}", new { status = "Active", modules = new[] { "Customers" } });
        Assert.Equal(HttpStatusCode.Forbidden, (await b.Send(HttpMethod.Get, "/api/vehicles")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await b.Send(HttpMethod.Get, "/api/services")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await b.Send(HttpMethod.Get, "/api/work-orders")).StatusCode);
        await using var scope = factory.Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Empty(await db.Customers.ToListAsync()); // Missing tenant fails closed.
        var member = await db.TenantUsers.SingleAsync(x => x.TenantId == betaId); member.Role = TenantRole.Member; await db.SaveChangesAsync();
        Assert.Equal(HttpStatusCode.Forbidden, (await b.Send(HttpMethod.Put, "/api/company", new { name = "Unauthorized" })).StatusCode);
        member.IsActive = false; await db.SaveChangesAsync();
        Assert.Equal(HttpStatusCode.Forbidden, (await b.Send(HttpMethod.Get, "/api/customers")).StatusCode);
    }

    private sealed record Records(Guid Customer, Guid Vehicle, Guid Service, Guid Part, JsonElement Order);
    private static object Customer(string name) => new { name, document = "12345678909" };
    private static object Vehicle(Guid customerId, string plate) => new { customerId, plate, model = "Modelo teste" };
    private static object Part() => new { name = "Filtro", code = "F-01", costPrice = 10, salePrice = 20 };
    private static object Order(Guid customerId, Guid vehicleId, Guid? catalogId = null) => new { customerId, vehicleId, services = new[] { new { catalogId, description = "Revisão", quantity = 1, unitPrice = 100 } }, parts = Array.Empty<object>() };
    private static async Task<Records> Seed(TenantTestSession session, string name, string plate)
    {
        var customer = (await session.Json(HttpMethod.Post, "/api/customers", Customer(name))).GetProperty("id").GetGuid();
        var vehicle = (await session.Json(HttpMethod.Post, "/api/vehicles", Vehicle(customer, plate))).GetProperty("id").GetGuid();
        var service = (await session.Json(HttpMethod.Post, "/api/services", new { name = "Revisão", defaultPrice = 100 })).GetProperty("id").GetGuid();
        var part = (await session.Json(HttpMethod.Post, "/api/parts", Part())).GetProperty("id").GetGuid();
        return new(customer, vehicle, service, part, await session.Json(HttpMethod.Post, "/api/work-orders", Order(customer, vehicle, service)));
    }
    private static async Task CheckIsolation(TenantTestSession session, Records other)
    {
        foreach (var (route, id, body) in new (string, Guid, object)[] { ("customers", other.Customer, Customer("Intruso")), ("vehicles", other.Vehicle, Vehicle(other.Customer, "ZZZ1A23")), ("services", other.Service, new { name = "Intruso", defaultPrice = 1 }), ("parts", other.Part, Part()), ("work-orders", other.Order.GetProperty("id").GetGuid(), Order(other.Customer, other.Vehicle)) })
        {
            var list = await session.Json(HttpMethod.Get, $"/api/{route}"); Assert.Equal(1, list.GetProperty("total").GetInt32());
            Assert.DoesNotContain(list.GetProperty("items").EnumerateArray(), x => x.GetProperty("id").GetGuid() == id);
            Assert.Equal(HttpStatusCode.NotFound, (await session.Send(HttpMethod.Get, $"/api/{route}/{id}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await session.Send(HttpMethod.Put, $"/api/{route}/{id}", body)).StatusCode);
            if (route != "work-orders") Assert.Equal(HttpStatusCode.NotFound, (await session.Send(HttpMethod.Delete, $"/api/{route}/{id}")).StatusCode);
            else { Assert.Equal(HttpStatusCode.NotFound, (await session.Send(HttpMethod.Get, $"/api/{route}/{id}/pdf")).StatusCode); Assert.Equal(HttpStatusCode.NotFound, (await session.Send(HttpMethod.Patch, $"/api/{route}/{id}/status", new { status = "InProgress" })).StatusCode); }
        }
    }
}
