using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SportPneus.IntegrationTests;

public sealed class WorkOrderFlowTests(SportPneusFactory factory) : IClassFixture<SportPneusFactory>
{
    [Fact]
    public async Task CalculatesTotalsPreservesSnapshotsAndEnforcesTransitions()
    {
        using var client = factory.CreateClient(new() { HandleCookies = false }); await Setup(client);
        var customerResponse = await client.PostAsJsonAsync("/api/customers", new { name = "Maria Oficina", document = (string?)null, phone = "11911112222", whatsApp = (string?)null, email = (string?)null, address = (string?)null, notes = (string?)null });
        var customerId = (await customerResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var vehicleResponse = await client.PostAsJsonAsync("/api/vehicles", new { customerId, plate = "DEF2G34", brand = "Fiat", model = "Uno", year = 2018, color = (string?)null, mileage = 50000, chassis = (string?)null, notes = (string?)null });
        var vehicleId = (await vehicleResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var serviceResponse = await client.PostAsJsonAsync("/api/services", new { name = "Alinhamento", description = (string?)null, defaultPrice = 100m }); var serviceId = (await serviceResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var partResponse = await client.PostAsJsonAsync("/api/parts", new { name = "Válvula", code = "VAL-1", costPrice = 5m, salePrice = 12.5m }); var partId = (await partResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var created = await client.PostAsJsonAsync("/api/work-orders", new { customerId, vehicleId, mileage = 51000, complaint = "Puxa para a direita", diagnosis = "Geometria", notes = (string?)null, services = new[] { new { catalogId = (Guid?)serviceId, description = "Alinhamento", quantity = 1m, unitPrice = 100m } }, parts = new[] { new { catalogId = (Guid?)partId, description = "Válvula", quantity = 4m, unitPrice = 12.5m } } });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode); var order = await created.Content.ReadFromJsonAsync<JsonElement>(); Assert.Equal(150m, order.GetProperty("total").GetDecimal()); Assert.True(order.GetProperty("number").GetInt64() > 0); var id = order.GetProperty("id").GetGuid();
        var updated = await client.PutAsJsonAsync($"/api/work-orders/{id}", new { customerId, vehicleId, mileage = 52000, complaint = "Puxa para a direita", diagnosis = "Geometria e balanceamento", notes = "Adicionado serviço extra", services = new[] { new { catalogId = (Guid?)serviceId, description = "Alinhamento", quantity = 1m, unitPrice = 120m } }, parts = new[] { new { catalogId = (Guid?)partId, description = "Válvula", quantity = 2m, unitPrice = 12.5m } } });
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode); var updatedOrder = await updated.Content.ReadFromJsonAsync<JsonElement>(); Assert.Equal(145m, updatedOrder.GetProperty("total").GetDecimal()); Assert.Equal(52000, updatedOrder.GetProperty("mileage").GetInt32());
        Assert.Equal(HttpStatusCode.OK, (await client.PatchAsJsonAsync($"/api/work-orders/{id}/status", new { status = "InProgress" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PatchAsJsonAsync($"/api/work-orders/{id}/status", new { status = "Completed" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PutAsJsonAsync($"/api/work-orders/{id}", new { customerId, vehicleId, mileage = 51000, complaint = "Alterada", diagnosis = (string?)null, notes = (string?)null, services = Array.Empty<object>(), parts = Array.Empty<object>() })).StatusCode);
    }

    private static async Task Setup(HttpClient client)
    {
        var status = await client.GetAsync("/api/setup/status"); var cookies = status.Headers.GetValues("Set-Cookie").ToList(); var xsrf = cookies.Single(x => x.StartsWith("XSRF-TOKEN=")).Split(';')[0]; var protection = cookies.Single(x => x.StartsWith("sport_xsrf_protection=")).Split(';')[0]; client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", Uri.UnescapeDataString(xsrf.Split('=', 2)[1])); client.DefaultRequestHeaders.Add("Cookie", $"{protection}; {xsrf}");
        var response = await client.PostAsJsonAsync("/api/setup", new { companyName = "Sport Pneus", cnpj = (string?)null, phone = (string?)null, adminName = "Administrador", email = "admin@sportpneus.local", password = "Oficina2026" }); Assert.Equal(HttpStatusCode.Created, response.StatusCode); var session = response.Headers.GetValues("Set-Cookie").Select(x => x.Split(';')[0]).Append(protection).Append(xsrf); client.DefaultRequestHeaders.Remove("Cookie"); client.DefaultRequestHeaders.Add("Cookie", string.Join("; ", session)); var authenticated = await client.GetAsync("/api/auth/me"); var authenticatedXsrf = authenticated.Headers.GetValues("Set-Cookie").Single(x => x.StartsWith("XSRF-TOKEN=")).Split(';')[0]; client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN"); client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", Uri.UnescapeDataString(authenticatedXsrf.Split('=', 2)[1]));
    }
}
