using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ofizzy.IntegrationTests;

public sealed class WorkOrderFlowTests(OfizzyFactory factory) : IClassFixture<OfizzyFactory>
{
    [Fact]
    public async Task CalculatesTotalsPreservesSnapshotsAndEnforcesTransitions()
    {
        // Arrange
        using var client = factory.CreateClient(new() { HandleCookies = false });
        await Setup(client);

        var customerPayload = new
        {
            name = "Maria Oficina",
            document = (string?)null,
            phone = "11911112222",
            whatsApp = (string?)null,
            email = (string?)null,
            address = (string?)null,
            notes = (string?)null
        };
        var customerResponse = await client.PostAsJsonAsync("/api/customers", customerPayload);
        var customerJson = await customerResponse.Content.ReadFromJsonAsync<JsonElement>();
        var customerId = customerJson.GetProperty("id").GetGuid();

        var vehiclePayload = new
        {
            customerId,
            plate = "DEF2G34",
            brand = "Fiat",
            model = "Uno",
            year = 2018,
            color = (string?)null,
            mileage = 50000,
            chassis = (string?)null,
            notes = (string?)null
        };
        var vehicleResponse = await client.PostAsJsonAsync("/api/vehicles", vehiclePayload);
        var vehicleJson = await vehicleResponse.Content.ReadFromJsonAsync<JsonElement>();
        var vehicleId = vehicleJson.GetProperty("id").GetGuid();

        var serviceResponse = await client.PostAsJsonAsync("/api/services", new
        {
            name = "Alinhamento",
            description = (string?)null,
            defaultPrice = 100m
        });
        var serviceJson = await serviceResponse.Content.ReadFromJsonAsync<JsonElement>();
        var serviceId = serviceJson.GetProperty("id").GetGuid();

        var partResponse = await client.PostAsJsonAsync("/api/parts", new
        {
            name = "Válvula",
            code = "VAL-1",
            costPrice = 5m,
            salePrice = 12.5m
        });
        var partJson = await partResponse.Content.ReadFromJsonAsync<JsonElement>();
        var partId = partJson.GetProperty("id").GetGuid();

        // Act - Create Work Order
        var createPayload = new
        {
            customerId,
            vehicleId,
            mileage = 51000,
            complaint = "Puxa para a direita",
            diagnosis = "Geometria",
            notes = (string?)null,
            services = new object[]
            {
                new { catalogId = (Guid?)serviceId, description = "Alinhamento", quantity = 1m, unitPrice = 100m },
                new { catalogId = (Guid?)null, description = "Desempeno de roda", quantity = 2m, unitPrice = 50m }
            },
            parts = new object[]
            {
                new { catalogId = (Guid?)partId, description = "Válvula", quantity = 4m, unitPrice = 12.5m },
                new { catalogId = (Guid?)null, description = "Abraçadeira inox", code = "ABR-01", quantity = 3m, unitPrice = 10m }
            }
        };

        var created = await client.PostAsJsonAsync("/api/work-orders", createPayload);

        // Assert - Created
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var order = await created.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(280m, order.GetProperty("total").GetDecimal());
        Assert.True(order.GetProperty("number").GetInt64() > 0);
        var id = order.GetProperty("id").GetGuid();

        // Act - Update Work Order
        var updatePayload = new
        {
            customerId,
            vehicleId,
            mileage = 52000,
            complaint = "Puxa para a direita",
            diagnosis = "Geometria e balanceamento",
            notes = "Adicionado serviço extra",
            services = new[]
            {
                new { catalogId = (Guid?)serviceId, description = "Alinhamento especial", quantity = 1m, unitPrice = 120m }
            },
            parts = new[]
            {
                new { catalogId = (Guid?)partId, description = "Válvula", quantity = 2m, unitPrice = 12.5m }
            }
        };

        var updated = await client.PutAsJsonAsync($"/api/work-orders/{id}", updatePayload);

        // Assert - Updated
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var updatedOrder = await updated.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(145m, updatedOrder.GetProperty("total").GetDecimal());
        Assert.Equal(52000, updatedOrder.GetProperty("mileage").GetInt32());

        // Status transitions
        var progressResponse = await client.PatchAsJsonAsync($"/api/work-orders/{id}/status", new { status = "InProgress" });
        Assert.Equal(HttpStatusCode.OK, progressResponse.StatusCode);

        var completedResponse = await client.PatchAsJsonAsync($"/api/work-orders/{id}/status", new { status = "Completed" });
        Assert.Equal(HttpStatusCode.OK, completedResponse.StatusCode);

        // Block edit when completed
        var lockedEditResponse = await client.PutAsJsonAsync($"/api/work-orders/{id}", new
        {
            customerId,
            vehicleId,
            mileage = 51000,
            complaint = "Alterada",
            diagnosis = (string?)null,
            notes = (string?)null,
            services = Array.Empty<object>(),
            parts = Array.Empty<object>()
        });
        Assert.Equal(HttpStatusCode.Conflict, lockedEditResponse.StatusCode);
    }

    private static Task Setup(HttpClient client) => TenantTestSession.BootstrapOperationalTenant(client);
}
