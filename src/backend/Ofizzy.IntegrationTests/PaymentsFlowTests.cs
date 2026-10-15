using System.Net;
using System.Text.Json;

namespace Ofizzy.IntegrationTests;

public sealed class PaymentsFlowTests(OfizzyFactory factory) : IClassFixture<OfizzyFactory>
{
    [Fact]
    public async Task Partial_payments_are_idempotent_persisted_and_isolated_and_reversals_preserve_history()
    {
        using var client = factory.CreateClient(new() { HandleCookies = false });
        var session = new TenantTestSession(client);
        await session.Bootstrap();
        var first = await session.Provision("Payments Alpha", "payments-alpha", "admin@ofizzy.local");
        var second = await session.Provision("Payments Beta", "payments-beta", "admin@ofizzy.local");
        await session.Select(first.GetProperty("id").GetGuid(), true);
        var customer = await session.Json(HttpMethod.Post, "/api/customers", new { name = "Cliente Pagamentos", document = "12.ABC.345/01de-35" });
        Assert.Equal("12ABC34501DE35", customer.GetProperty("document").GetString());
        var found = await session.Json(HttpMethod.Get, "/api/customers?q=12abc34501de35");
        Assert.Equal(1, found.GetProperty("total").GetInt32());
        var vehicle = await session.Json(HttpMethod.Post, "/api/vehicles", new { customerId = customer.GetProperty("id").GetGuid(), plate = "PAY1A23", brand = "Teste", model = "Teste", year = 2020 });
        var order = await session.Json(HttpMethod.Post, "/api/work-orders", new
        {
            customerId = customer.GetProperty("id").GetGuid(), vehicleId = vehicle.GetProperty("id").GetGuid(),
            services = new[] { new { description = "Serviço financeiro teste", quantity = 1m, unitPrice = 100m } },
            parts = Array.Empty<object>()
        });
        var id = order.GetProperty("id").GetGuid();
        var route = $"/api/work-orders/{id}/payments";
        var at = DateTimeOffset.UtcNow.AddMinutes(-3);
        var requestId = Guid.NewGuid();
        var payload = new { requestId, amount = 60m, method = "Pix", receivedAt = at };
        Assert.Equal(HttpStatusCode.Conflict, (await session.Send(HttpMethod.Post, route, payload)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await session.Send(HttpMethod.Patch, $"/api/work-orders/{id}/status", new { status = "InProgress" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await session.Send(HttpMethod.Patch, $"/api/work-orders/{id}/status", new { status = "Completed" })).StatusCode);
        var payment = await session.Json(HttpMethod.Post, route, payload);
        var paymentId = payment.GetProperty("id").GetGuid();
        Assert.Equal(paymentId, (await session.Json(HttpMethod.Post, route, payload)).GetProperty("id").GetGuid());
        Assert.Equal(HttpStatusCode.Conflict, (await session.Send(HttpMethod.Post, route, new { requestId, amount = 61m, method = "Pix", receivedAt = at })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await session.Send(HttpMethod.Post, route, new { requestId = Guid.NewGuid(), amount = 41m, method = "Cash", receivedAt = at })).StatusCode);
        var settleRoute = $"/api/payments/{paymentId}/settlements";
        var settlementPayload = new { requestId = Guid.NewGuid(), amount = 30m, fees = 1m, segregatedTax = (decimal?)null, settledAt = at.AddMinutes(1) };
        var movement = await session.Json(HttpMethod.Post, settleRoute, settlementPayload);
        var movementId = movement.GetProperty("id").GetGuid();
        Assert.Equal(movementId, (await session.Json(HttpMethod.Post, settleRoute, settlementPayload)).GetProperty("id").GetGuid());
        Assert.Equal(HttpStatusCode.Conflict, (await session.Send(HttpMethod.Post, settleRoute,
            new { requestId = Guid.NewGuid(), amount = 31m, fees = 0, settledAt = at.AddMinutes(1) })).StatusCode);
        var reversalRoute = $"/api/payments/{paymentId}/reversals";
        var reversalPayload = new { requestId = Guid.NewGuid(), settlementId = movementId, amount = 10m,
            reason = "Estorno manual confirmado pelo operador", reversedAt = at.AddMinutes(2) };
        var reversal = await session.Json(HttpMethod.Post, reversalRoute, reversalPayload);
        Assert.Equal(reversal.GetProperty("id").GetGuid(), (await session.Json(HttpMethod.Post, reversalRoute, reversalPayload)).GetProperty("id").GetGuid());
        Assert.Equal(HttpStatusCode.Conflict, (await session.Send(HttpMethod.Post, reversalRoute,
            new { requestId = Guid.NewGuid(), settlementId = movementId, amount = 21m,
                reason = "Tentativa excedendo saldo da liquidação", reversedAt = at.AddMinutes(2) })).StatusCode);
        var list = await session.Json(HttpMethod.Get, route);
        Assert.Equal(80m, list.GetProperty("balance").GetDecimal());
        Assert.Equal(2, list.GetProperty("payments")[0].GetProperty("movements").GetArrayLength());
        Assert.Equal(JsonValueKind.Null, list.GetProperty("payments")[0].GetProperty("movements")[0].GetProperty("segregatedTax").ValueKind);
        Assert.Equal(1, list.GetProperty("payments").GetArrayLength());
        Assert.Equal(HttpStatusCode.Conflict, (await session.Send(HttpMethod.Post, route,
            new { requestId = Guid.NewGuid(), amount = 1m, method = "Pix", receivedAt = at,
                allocations = new[] { new { documentId = Guid.NewGuid(), amount = 1m } } })).StatusCode);
        var fractionalOrder = await session.Json(HttpMethod.Post, "/api/work-orders", new
        {
            customerId = customer.GetProperty("id").GetGuid(), vehicleId = vehicle.GetProperty("id").GetGuid(),
            services = new[] { new { description = "Fração serviço A", quantity = .01m, unitPrice = 1.5m },
                new { description = "Fração serviço B", quantity = .01m, unitPrice = 1.5m } },
            parts = new[] { new { description = "Fração peça A", quantity = .01m, unitPrice = 1.5m },
                new { description = "Fração peça B", quantity = .01m, unitPrice = 1.5m } }
        });
        var fractionalRoute = $"/api/work-orders/{fractionalOrder.GetProperty("id").GetGuid()}/payments";
        Assert.Equal(.06m, (await session.Json(HttpMethod.Get, fractionalRoute)).GetProperty("total").GetDecimal());
        await session.Select(second.GetProperty("id").GetGuid(), true);
        Assert.Equal(HttpStatusCode.NotFound, (await session.Send(HttpMethod.Get, route)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await session.Send(HttpMethod.Post, settleRoute, settlementPayload)).StatusCode);
        await session.Select(first.GetProperty("id").GetGuid());
        Assert.Equal(80m, (await session.Json(HttpMethod.Get, route)).GetProperty("balance").GetDecimal());
    }
}
