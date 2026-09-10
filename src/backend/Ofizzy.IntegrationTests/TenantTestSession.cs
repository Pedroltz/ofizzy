using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ofizzy.IntegrationTests;

internal sealed class TenantTestSession(HttpClient client)
{
    private readonly Dictionary<string, string> cookies = [];
    public HttpClient Client => client;
    public async Task<HttpResponseMessage> Send(HttpMethod method, string url, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null) request.Content = JsonContent.Create(body);
        var response = await client.SendAsync(request);
        if (response.Headers.TryGetValues("Set-Cookie", out var headers))
            foreach (var header in headers) { var pair = header.Split(';')[0].Split('=', 2); cookies[pair[0]] = pair[1]; }
        client.DefaultRequestHeaders.Remove("Cookie"); client.DefaultRequestHeaders.Add("Cookie", string.Join("; ", cookies.Select(x => $"{x.Key}={x.Value}")));
        if (cookies.TryGetValue("XSRF-TOKEN", out var token)) { client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN"); client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", Uri.UnescapeDataString(token)); }
        return response;
    }
    public async Task<JsonElement> Json(HttpMethod method, string url, object? body = null)
    {
        var response = await Send(method, url, body);
        Assert.True(response.IsSuccessStatusCode, $"{method} {url}: {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
    public async Task Bootstrap()
    {
        await Json(HttpMethod.Get, "/api/setup/status");
        await Json(HttpMethod.Post, "/api/setup", new { companyName = "Ofizzy Platform", adminName = "Operador", email = "admin@ofizzy.local", password = "Oficina2026" });
        await Json(HttpMethod.Get, "/api/auth/me");
    }
    public Task<JsonElement> Provision(string name, string slug, string email, string? password = null, string[]? modules = null) => Json(HttpMethod.Post, "/api/platform/tenants", new { name, slug, vertical = "Automotive", adminName = "Proprietário", email, password, modules = modules ?? ["Customers", "WorkOrders", "Catalog", "Automotive"] });
    public async Task Select(Guid id, bool onboard = false)
    {
        await Json(HttpMethod.Post, "/api/auth/tenant", new { tenantId = id });
        await Json(HttpMethod.Get, "/api/auth/me");
        if (onboard) Assert.Equal(HttpStatusCode.NoContent, (await Send(HttpMethod.Post, "/api/tenant/onboarding/complete", new { })).StatusCode);
    }
    public async Task Login(string email, string password = "Oficina2026")
    {
        await Json(HttpMethod.Get, "/api/setup/status");
        await Json(HttpMethod.Post, "/api/auth/login", new { email, password });
        await Json(HttpMethod.Get, "/api/auth/me");
    }
    public static async Task BootstrapOperationalTenant(HttpClient client)
    {
        var session = new TenantTestSession(client); await session.Bootstrap();
        var tenant = await session.Provision("Ofizzy", "ofizzy", "admin@ofizzy.local");
        await session.Select(tenant.GetProperty("id").GetGuid(), onboard: true);
    }
}
