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

    [Fact]
    public async Task CustomerSearchFindsNameAndMaskedPhoneUsingNormalizedContact()
    {
        using var client = factory.CreateClient(new()
        {
            HandleCookies = false
        });
        await Setup(client);

        var create = await client.PostAsJsonAsync("/api/customers", new
        {
            name = "João da Silva",
            phone = "(11) 99999-9999"
        });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("11999999999", created.GetProperty("phone").GetString());

        var byName = await client.GetFromJsonAsync<JsonElement>("/api/customers?q=Jo%C3%A3o");
        var byPhone = await client.GetFromJsonAsync<JsonElement>("/api/customers?q=11%2099999-9999");

        Assert.Equal(1, byName.GetProperty("total").GetInt32());
        Assert.Equal(1, byPhone.GetProperty("total").GetInt32());
    }

    private static async Task Setup(HttpClient client)
    {
        var session = new TenantTestSession(client);
        var status = await session.Json(HttpMethod.Get, "/api/setup/status");
        if (status.GetProperty("required").GetBoolean())
            await session.Bootstrap();
        else
            await session.Login("admin@ofizzy.local");
        var slug = $"catalog-{Guid.NewGuid():N}";
        var tenant = await session.Provision("Catalog Test", slug, "admin@ofizzy.local");
        await session.Select(tenant.GetProperty("id").GetGuid(), onboard: true);
    }
}
