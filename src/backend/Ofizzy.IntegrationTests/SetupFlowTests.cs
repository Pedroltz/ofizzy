using System.Net;
using System.Net.Http.Json;

namespace Ofizzy.IntegrationTests;

public sealed class SetupFlowTests(OfizzyFactory factory) : IClassFixture<OfizzyFactory>
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
        var request = new { companyName = "Ofizzy", cnpj = "12.345.678/0001-90", phone = "11999999999", adminName = "Administrador", email = "admin@ofizzy.local", password = "Oficina2026" };

        var created = await client.PostAsJsonAsync("/api/setup", request);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        using var otherClient = factory.CreateClient();
        var otherStatus = await otherClient.GetAsync("/api/setup/status");
        var otherCookie = otherStatus.Headers.GetValues("Set-Cookie").Single(value => value.StartsWith("XSRF-TOKEN="));
        otherClient.DefaultRequestHeaders.Add("X-XSRF-TOKEN", Uri.UnescapeDataString(otherCookie.Split(';')[0].Split('=', 2)[1]));
        var duplicate = await otherClient.PostAsJsonAsync("/api/setup", request);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    private sealed record SetupStatus(bool Required);
}
