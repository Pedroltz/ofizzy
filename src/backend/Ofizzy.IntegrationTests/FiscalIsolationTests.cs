using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Modules.Tenancy;

namespace Ofizzy.IntegrationTests;

public sealed class FiscalIsolationTests(OfizzyFactory factory) : IClassFixture<OfizzyFactory>
{
    [Fact]
    public async Task Preparation_persists_and_fiscal_routes_enforce_tenant_and_role()
    {
        using var operatorClient = factory.CreateClient(new() { HandleCookies = false });
        var platform = new TenantTestSession(operatorClient);
        await platform.Bootstrap();
        var alpha = (await platform.Provision("Fiscal Alpha", "fiscal-alpha", "fiscal-a@example.test", "Oficina2026")).GetProperty("id").GetGuid();
        var beta = (await platform.Provision("Fiscal Beta", "fiscal-beta", "fiscal-b@example.test", "Oficina2026")).GetProperty("id").GetGuid();
        using var aClient = factory.CreateClient(new() { HandleCookies = false });
        using var bClient = factory.CreateClient(new() { HandleCookies = false });
        var a = new TenantTestSession(aClient); var b = new TenantTestSession(bClient);
        await a.Login("fiscal-a@example.test"); await a.Select(alpha, true);
        await b.Login("fiscal-b@example.test"); await b.Select(beta, true);
        var customer = (await a.Json(HttpMethod.Post, "/api/customers", new { name = "Cliente Fiscal", document = "12345678909" })).GetProperty("id");
        var vehicle = (await a.Json(HttpMethod.Post, "/api/vehicles", new { customerId = customer, plate = "FIS1A23", model = "Teste" })).GetProperty("id");
        var order = (await a.Json(HttpMethod.Post, "/api/work-orders", new
        {
            customerId = customer,
            vehicleId = vehicle,
            services = new[] { new { description = "Revisão", quantity = 1, unitPrice = 100 } },
            parts = Array.Empty<object>()
        })).GetProperty("id").GetGuid();
        var route = $"/api/work-orders/{order}/fiscal";
        Assert.Equal(HttpStatusCode.NoContent, (await a.Send(HttpMethod.Put, route, new { name = "Tomador preservado", document = "12345678909" })).StatusCode);
        // A new request uses a new DbContext and reads the PostgreSQL record.
        var prepared = await a.Json(HttpMethod.Get, route);
        Assert.Equal("Tomador preservado", prepared.GetProperty("preparation").GetProperty("name").GetString());
        Assert.NotEmpty(prepared.GetProperty("issues").EnumerateArray());
        Assert.Equal(HttpStatusCode.BadRequest, (await a.Send(HttpMethod.Post, route + "/issue")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await a.Send(HttpMethod.Get, route + "/download")).StatusCode);
        foreach (var method in new[] { HttpMethod.Get, HttpMethod.Put })
            Assert.Equal(HttpStatusCode.NotFound, (await b.Send(method, route, method == HttpMethod.Put ? new { name = "Intruso" } : null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.Send(HttpMethod.Post, route + "/issue")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.Send(HttpMethod.Get, route + "/download")).StatusCode);
        var invalid = new { cnpj = "11111111111111", legalName = "Inválido" };
        Assert.Equal(HttpStatusCode.BadRequest, (await a.Send(HttpMethod.Put, "/api/fiscal/settings", invalid)).StatusCode);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Empty(await db.FiscalPreparationEntries.ToListAsync());
        var member = await db.TenantUsers.SingleAsync(x => x.TenantId == alpha);
        member.Role = TenantRole.Member; await db.SaveChangesAsync();
        Assert.Equal(HttpStatusCode.Forbidden, (await a.Send(HttpMethod.Get, "/api/fiscal/settings")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await a.Send(HttpMethod.Put, "/api/fiscal/settings", invalid)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await a.Send(HttpMethod.Get, "/api/fiscal/nfe/inutilizations")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await a.Send(HttpMethod.Get, route)).StatusCode);
    }
}
