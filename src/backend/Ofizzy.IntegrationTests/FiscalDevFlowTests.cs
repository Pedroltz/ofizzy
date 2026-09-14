using System.Net;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Modules.Fiscal;
using Ofizzy.Api.Modules.Tenancy;

namespace Ofizzy.IntegrationTests;

public sealed class FiscalDevFlowTests(OfizzyFactory factory) : IClassFixture<OfizzyFactory>
{
    [Fact]
    public async Task Simulated_flow_uses_real_api_certificate_schema_persistence_and_permissions()
    {
        using var host = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string, string?>
        { ["Fiscal:SimulateGateway"] = "true", ["Fiscal:ActiveKeyId"] = "test", ["Fiscal:Keys:test"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)) })));
        using var client = host.CreateClient(new() { HandleCookies = false });
        var session = new TenantTestSession(client);
        await session.Bootstrap();
        Assert.Equal(HttpStatusCode.Forbidden, (await session.Send(HttpMethod.Get, "/api/fiscal/dev/certificate")).StatusCode);
        var tenantId = (await session.Provision("Fiscal Simulado", "fiscal-simulado", "admin@ofizzy.local")).GetProperty("id").GetGuid();
        await session.Select(tenantId, true);
        var address = new FiscalAddress("Rua do Emitente", "10", "Centro", "Igaraçu do Tietê", "3520004", "SP", "17350000");
        var settings = new FiscalSettingsData("11222333000181", "OFICINA FICTICIA", "110042490114", Regime: "SimplesNacional", Address: address, NfeEnabled: true, NfseEnabled: true);
        Assert.Equal(HttpStatusCode.NoContent, (await session.Send(HttpMethod.Put, "/api/fiscal/settings", settings)).StatusCode);
        Assert.True((await session.Json(HttpMethod.Get, "/api/fiscal/settings")).GetProperty("devToolsAvailable").GetBoolean());
        var certificate = await session.Send(HttpMethod.Get, "/api/fiscal/dev/certificate");
        Assert.Equal(HttpStatusCode.OK, certificate.StatusCode);
        Assert.True(certificate.Headers.CacheControl!.NoStore);
        using var upload = new MultipartFormDataContent();
        upload.Add(new ByteArrayContent(await certificate.Content.ReadAsByteArrayAsync()), "file", "ficticio.pfx");
        upload.Add(new StringContent("teste123"), "password");
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/fiscal/certificate", upload)).StatusCode);
        var customer = (await session.Json(HttpMethod.Post, "/api/customers", new { name = "Cliente Fictício", document = "12345678909" })).GetProperty("id");
        var vehicle = (await session.Json(HttpMethod.Post, "/api/vehicles", new { customerId = customer, plate = "DEV1A23", model = "Teste" })).GetProperty("id");
        var order = await session.Json(HttpMethod.Post, "/api/work-orders", new { customerId = customer, vehicleId = vehicle,
            services = new[] { new { description = "Revisão", quantity = 1, unitPrice = 100 } }, parts = new[] { new { description = "Pneu", code = "PN-01", quantity = 1, unitPrice = 300 } } });
        var id = order.GetProperty("id").GetGuid();
        foreach (var status in new[] { "InProgress", "Completed" })
            Assert.True((await session.Send(HttpMethod.Patch, $"/api/work-orders/{id}/status", new { status })).IsSuccessStatusCode);
        var preparation = new FiscalPreparationData(address with { Street = "Rua do Tomador" }, "12345678909", "Cliente Fictício", Competence: DateOnly.FromDateTime(DateTime.UtcNow), PaymentCode: "01", PaymentAmount: 300,
            Products: new() { [order.GetProperty("parts")[0].GetProperty("id").GetGuid()] = new("40111000", Cfop: "5102", Csosn: "102", PisCst: "07", CofinsCst: "07") },
            Services: new() { [order.GetProperty("services")[0].GetProperty("id").GetGuid()] = new("140101", ApproximateTaxRate: 6) });
        var route = $"/api/work-orders/{id}/fiscal";
        Assert.Equal(HttpStatusCode.NoContent, (await session.Send(HttpMethod.Put, route, preparation)).StatusCode);
        var issued = await session.Send(HttpMethod.Post, route + "/issue");
        Assert.True(issued.IsSuccessStatusCode, await issued.Content.ReadAsStringAsync());
        var result = await session.Json(HttpMethod.Get, route);
        Assert.Equal("Completed", result.GetProperty("status").GetString());
        foreach (var doc in result.GetProperty("documents").EnumerateArray())
        {
            var docRoute = $"/api/fiscal/documents/{doc.GetProperty("id").GetGuid()}";
            var xml = await (await session.Send(HttpMethod.Get, docRoute + "/xml")).Content.ReadAsStringAsync();
            if (doc.GetProperty("kind").GetString() == "Nfse")
            {
                Assert.Matches("^[0-9]{50}$", doc.GetProperty("accessKey").GetString()!);
                FiscalXml.ValidateAuthorizedNfse(xml);
                Assert.Equal("Cliente Fictício", FiscalXml.Parse(xml).Descendants(FiscalXml.Nfse + "toma").Single().Element(FiscalXml.Nfse + "xNome")!.Value);
            }
            Assert.Equal(HttpStatusCode.OK, (await session.Send(HttpMethod.Get, docRoute + "/pdf")).StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, (await session.Send(HttpMethod.Post, docRoute + "/sync")).StatusCode);
            Assert.Equal(xml, await (await session.Send(HttpMethod.Get, docRoute + "/xml")).Content.ReadAsStringAsync());
            var cancelled = await session.Send(HttpMethod.Post, docRoute + "/cancel", new { reason = "Cancelamento fictício para teste do fluxo completo" });
            Assert.True(cancelled.IsSuccessStatusCode, await cancelled.Content.ReadAsStringAsync());
            Assert.Equal("Cancelled", (await session.Json(HttpMethod.Get, docRoute)).GetProperty("state").GetString());
            Assert.Equal(HttpStatusCode.OK, (await session.Send(HttpMethod.Get, docRoute + "/pdf")).StatusCode);
        }
        var config = host.Services.GetRequiredService<IConfiguration>();
        config["Fiscal:SimulateGateway"] = "false";
        Assert.False((await session.Json(HttpMethod.Get, "/api/fiscal/settings")).GetProperty("devToolsAvailable").GetBoolean());
        Assert.Equal(HttpStatusCode.NotFound, (await session.Send(HttpMethod.Get, "/api/fiscal/dev/certificate")).StatusCode);
        config["Fiscal:SimulateGateway"] = "true";
        config["Fiscal:ProductionEnabled"] = "true";
        Assert.Equal(HttpStatusCode.NoContent, (await session.Send(HttpMethod.Put, "/api/fiscal/settings", settings with { Environment = FiscalEnvironment.Production })).StatusCode);
        Assert.False((await session.Json(HttpMethod.Get, "/api/fiscal/settings")).GetProperty("devToolsAvailable").GetBoolean());
        Assert.Equal(HttpStatusCode.NotFound, (await session.Send(HttpMethod.Get, "/api/fiscal/dev/certificate")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await session.Send(HttpMethod.Put, "/api/fiscal/settings", settings)).StatusCode);
        var environment = host.Services.GetRequiredService<IWebHostEnvironment>();
        environment.EnvironmentName = "Production";
        Assert.False((await session.Json(HttpMethod.Get, "/api/fiscal/settings")).GetProperty("devToolsAvailable").GetBoolean());
        Assert.Equal(HttpStatusCode.NotFound, (await session.Send(HttpMethod.Get, "/api/fiscal/dev/certificate")).StatusCode);
        environment.EnvironmentName = "Development";
        await using var scope = host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var membership = await db.TenantUsers.SingleAsync(x => x.TenantId == tenantId);
        membership.Role = TenantRole.Member; await db.SaveChangesAsync();
        Assert.Equal(HttpStatusCode.Forbidden, (await session.Send(HttpMethod.Get, "/api/fiscal/dev/certificate")).StatusCode);
    }
}
