using System.Net;
using System.Net.Http.Json;

namespace SportPneus.IntegrationTests;

public sealed class SetupFlowTests(SportPneusFactory factory) : IClassFixture<SportPneusFactory>
{
    [Fact]
    public async Task SetupCanOnlyBeCompletedOnce()
    {
        using var client = factory.CreateClient();
        var status = await client.GetAsync("/api/setup/status");
        Assert.True(status.IsSuccessStatusCode, await status.Content.ReadAsStringAsync());
        Assert.True((await status.Content.ReadFromJsonAsync<SetupStatus>())!.Required);

        var xsrfCookie = status.Headers.GetValues("Set-Cookie").Single(value => value.StartsWith("XSRF-TOKEN="));
        var xsrf = Uri.UnescapeDataString(xsrfCookie.Split(';')[0].Split('=', 2)[1]);
        client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", xsrf);
        var request = new { companyName = "Sport Pneus", cnpj = (string?)null, phone = "11999999999", adminName = "Administrador", email = "admin@sportpneus.local", password = "Oficina2026" };

        var created = await client.PostAsJsonAsync("/api/setup", request);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var duplicate = await client.PostAsJsonAsync("/api/setup", request);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    private sealed record SetupStatus(bool Required);
}
