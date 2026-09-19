using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ofizzy.IntegrationTests;

public sealed class CatalogFlowTests(OfizzyFactory factory) : IClassFixture<OfizzyFactory>
{
    [Fact]
    public async Task AuthenticatedUserCanManageCatalogsAndArchiveCustomerVehicles()
    {
        // Arrange
        using var client = factory.CreateClient(new()
        {
            HandleCookies = false
        });
        await Setup(client);

        // Act - Create Customer
        var customerPayload = new
        {
            name = "João da Silva",
            document = "12345678909",
            phone = "11999999999",
            whatsApp = "11999999999",
            email = "joao@example.com",
            address = "Rua das Oficinas, 10",
            notes = "Cliente recorrente"
        };
        var customerResponse = await client.PostAsJsonAsync("/api/customers", customerPayload);

        // Assert - Customer Created
        Assert.True(customerResponse.StatusCode == HttpStatusCode.Created, await customerResponse.Content.ReadAsStringAsync());
        var customer = await customerResponse.Content.ReadFromJsonAsync<JsonElement>();
        var customerId = customer.GetProperty("id").GetGuid();

        // Act - Create Vehicle
        var vehiclePayload = new
        {
            customerId,
            plate = "abc1d23",
            brand = "Chevrolet",
            model = "Corsa",
            year = 2020,
            color = "Prata",
            mileage = 100000,
            chassis = (string?)null,
            notes = (string?)null
        };
        var vehicleResponse = await client.PostAsJsonAsync("/api/vehicles", vehiclePayload);

        // Assert - Vehicle Created
        Assert.Equal(HttpStatusCode.Created, vehicleResponse.StatusCode);
        var createdVehicle = await vehicleResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("ABC1D23", createdVehicle.GetProperty("plate").GetString());

        // Act & Assert - Services & Parts
        var serviceResponse = await client.PostAsJsonAsync("/api/services", new
        {
            name = "Troca de óleo",
            description = "Óleo e filtro",
            defaultPrice = 120m
        });
        Assert.Equal(HttpStatusCode.Created, serviceResponse.StatusCode);

        var part = new
        {
            name = "Filtro de óleo",
            code = "flt-001",
            costPrice = 20m,
            salePrice = 40m
        };
        var partResponse = await client.PostAsJsonAsync("/api/parts", part);
        Assert.Equal(HttpStatusCode.Created, partResponse.StatusCode);

        var duplicatePartResponse = await client.PostAsJsonAsync("/api/parts", part);
        Assert.Equal(HttpStatusCode.Conflict, duplicatePartResponse.StatusCode);

        // Query Vehicles
        var vehicles = await client.GetFromJsonAsync<JsonElement>("/api/vehicles?q=ABC");
        Assert.Equal(1, vehicles.GetProperty("total").GetInt32());

        // Act - Archive Customer
        var deleteResponse = await client.DeleteAsync($"/api/customers/{customerId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Assert - Cascading Vehicles Inactive
        vehicles = await client.GetFromJsonAsync<JsonElement>("/api/vehicles");
        Assert.Equal(0, vehicles.GetProperty("total").GetInt32());
    }

    private static Task Setup(HttpClient client) => TenantTestSession.BootstrapOperationalTenant(client);
}
