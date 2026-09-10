using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ofizzy.IntegrationTests;

public sealed class CatalogFlowTests(OfizzyFactory factory) : IClassFixture<OfizzyFactory>
{
    [Fact]
    public async Task AuthenticatedUserCanManageCatalogsAndArchiveCustomerVehicles()
    {
        using var client = factory.CreateClient(new()
        {
            HandleCookies = false
        });
        await Setup(client);

        var customerResponse = await client.PostAsJsonAsync("/api/customers", new
        {
            name = "João da Silva",
            document = "12345678909",
            phone = "11999999999",
            whatsApp = "11999999999",
            email = "joao@example.com",
            address = "Rua das Oficinas, 10",
            notes = "Cliente recorrente"
        });
        Assert.True(customerResponse.StatusCode == HttpStatusCode.Created, await customerResponse.Content.ReadAsStringAsync());
        var customer = await customerResponse.Content.ReadFromJsonAsync<JsonElement>(); var customerId = customer.GetProperty("id").GetGuid();

        var vehicleResponse = await client.PostAsJsonAsync("/api/vehicles", new { customerId, plate = "abc1d23", brand = "Chevrolet", model = "Corsa", year = 2020, color = "Prata", mileage = 100000, chassis = (string?)null, notes = (string?)null });
        Assert.Equal(HttpStatusCode.Created, vehicleResponse.StatusCode);
        Assert.Equal("ABC1D23", (await vehicleResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("plate").GetString());

        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/services", new { name = "Troca de óleo", description = "Óleo e filtro", defaultPrice = 120m })).StatusCode);
        var part = new { name = "Filtro de óleo", code = "flt-001", costPrice = 20m, salePrice = 40m };
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/parts", part)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/parts", part)).StatusCode);

        var vehicles = await client.GetFromJsonAsync<JsonElement>("/api/vehicles?q=ABC");
        Assert.Equal(1, vehicles.GetProperty("total").GetInt32());
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/customers/{customerId}")).StatusCode);
        vehicles = await client.GetFromJsonAsync<JsonElement>("/api/vehicles");
        Assert.Equal(0, vehicles.GetProperty("total").GetInt32());
    }

    private static Task Setup(HttpClient client) => TenantTestSession.BootstrapOperationalTenant(client);
}
