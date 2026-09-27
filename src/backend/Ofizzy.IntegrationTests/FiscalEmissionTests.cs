using System.Formats.Asn1;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Ofizzy.Api.Modules.Fiscal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ofizzy.IntegrationTests;

public sealed class FiscalEmissionTests(OfizzyFactory factory) : IClassFixture<OfizzyFactory>
{
    [Fact]
    public async Task Mixed_order_reconciles_timeout_without_reissuing_authorized_product_invoice()
    {
        // Arrange
        var gateway = new TimeoutGateway();
        using var host = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Fiscal:ActiveKeyId"] = "test",
                ["Fiscal:Keys:test"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            }));
            builder.ConfigureServices(services => services.AddSingleton<IFiscalGateway>(gateway));
        });

        using var client = host.CreateClient(new() { HandleCookies = false });
        var session = new TenantTestSession(client);
        await session.Bootstrap();

        var tenant = await session.Provision("Fiscal Emissão", "fiscal-emissao", "admin@ofizzy.local");
        await session.Select(tenant.GetProperty("id").GetGuid(), true);

        var address = new FiscalAddress("Rua Teste", "10", "Centro", "São Paulo", "3550308", "SP", "01001000");
        var settings = new FiscalSettingsData(
            "11222333000181",
            "OFICINA FICTICIA",
            "110042490114",
            Regime: "SimplesNacional",
            Address: address,
            NfeEnabled: true,
            NfseEnabled: true);

        var settingsResponse = await session.Send(HttpMethod.Put, "/api/fiscal/settings", settings);
        Assert.Equal(HttpStatusCode.NoContent, settingsResponse.StatusCode);

        using var cert = Certificate();
        using var upload = new MultipartFormDataContent();
        upload.Add(new ByteArrayContent(cert.Export(X509ContentType.Pkcs12, "test-password")), "file", "test.pfx");
        upload.Add(new StringContent("test-password"), "password");

        var uploaded = await client.PostAsync("/api/fiscal/certificate", upload);
        Assert.True(uploaded.IsSuccessStatusCode, await uploaded.Content.ReadAsStringAsync());

        var customer = (await session.Json(HttpMethod.Post, "/api/customers", new { name = "Cliente Fiscal", document = "12345678909" })).GetProperty("id");
        var vehicle = (await session.Json(HttpMethod.Post, "/api/vehicles", new { customerId = customer, plate = "FIS1A23", model = "Teste" })).GetProperty("id");

        var order = await session.Json(HttpMethod.Post, "/api/work-orders", new
        {
            customerId = customer,
            vehicleId = vehicle,
            services = new[] { new { description = "Revisão", quantity = 1, unitPrice = 100 } },
            parts = new[] { new { description = "Pneu", code = "PN-01", quantity = 1, unitPrice = 300 } }
        });

        var id = order.GetProperty("id").GetGuid();
        foreach (var status in new[] { "InProgress", "Completed" })
        {
            var patchStatus = await session.Send(HttpMethod.Patch, $"/api/work-orders/{id}/status", new { status });
            Assert.True(patchStatus.IsSuccessStatusCode);
        }

        var partId = order.GetProperty("parts")[0].GetProperty("id").GetGuid();
        var serviceId = order.GetProperty("services")[0].GetProperty("id").GetGuid();

        var preparation = new FiscalPreparationData(
            Address: address,
            Document: "12345678909",
            Name: "Cliente Fiscal",
            Competence: DateOnly.FromDateTime(DateTime.UtcNow),
            PaymentCode: "01",
            PaymentAmount: 300,
            Products: new Dictionary<Guid, ProductFiscalData>
            {
                [partId] = new("40111000", Cfop: "5102", Csosn: "102", PisCst: "07", CofinsCst: "07")
            },
            Services: new Dictionary<Guid, ServiceFiscalData>
            {
                [serviceId] = new("140101", ApproximateTaxRate: 6)
            });

        var route = $"/api/work-orders/{id}/fiscal";
        var savePrep = await session.Send(HttpMethod.Put, route, preparation);
        Assert.Equal(HttpStatusCode.NoContent, savePrep.StatusCode);

        var prepResult = await session.Json(HttpMethod.Get, route);
        Assert.Empty(prepResult.GetProperty("issues").EnumerateArray());

        // First attempt (Service sends timeout)
        var first = await session.Send(HttpMethod.Post, $"{route}/issue");
        Assert.True(first.IsSuccessStatusCode, await first.Content.ReadAsStringAsync());

        var partial = await session.Json(HttpMethod.Get, route);
        Assert.Equal("Partial", partial.GetProperty("status").GetString());

        var conflictOnEdit = await session.Send(HttpMethod.Put, route, preparation);
        Assert.Equal(HttpStatusCode.Conflict, conflictOnEdit.StatusCode);

        // Second attempt
        var second = await session.Send(HttpMethod.Post, $"{route}/issue");
        Assert.True(second.IsSuccessStatusCode);

        var completed = await session.Json(HttpMethod.Get, route);
        Assert.Equal("Completed", completed.GetProperty("status").GetString());

        // Redundant issue attempt
        var redundant = await session.Send(HttpMethod.Post, $"{route}/issue");
        Assert.True(redundant.IsSuccessStatusCode);

        Assert.Equal(1, gateway.ProductSends);
        Assert.Equal(2, gateway.ServiceSends);
        Assert.Single(gateway.ServicePayloads.Distinct());

        var bundle = await session.Send(HttpMethod.Get, $"{route}/download");
        Assert.Equal(HttpStatusCode.OK, bundle.StatusCode);
        using var archive = new ZipArchive(await bundle.Content.ReadAsStreamAsync());
        Assert.Equal(2, archive.Entries.Count);

        // A separately rejected product invoice can be invalidated and recovered after a timeout.
        gateway.RejectProducts = true;
        var rejected = await session.Json(HttpMethod.Post, "/api/work-orders", new
        {
            customerId = customer,
            vehicleId = vehicle,
            services = Array.Empty<object>(),
            parts = new[] { new { description = "Pneu", code = "PN-02", quantity = 1, unitPrice = 300 } }
        });

        var rejectedId = rejected.GetProperty("id").GetGuid();
        foreach (var status in new[] { "InProgress", "Completed" })
        {
            var patchStatus = await session.Send(HttpMethod.Patch, $"/api/work-orders/{rejectedId}/status", new { status });
            Assert.True(patchStatus.IsSuccessStatusCode);
        }

        var rejectedRoute = $"/api/work-orders/{rejectedId}/fiscal";
        var rejectedPartId = rejected.GetProperty("parts")[0].GetProperty("id").GetGuid();
        var rejectedPreparation = preparation with
        {
            Services = null,
            Products = new Dictionary<Guid, ProductFiscalData>
            {
                [rejectedPartId] = preparation.Products!.Values.Single()
            }
        };

        var putRejected = await session.Send(HttpMethod.Put, rejectedRoute, rejectedPreparation);
        Assert.Equal(HttpStatusCode.NoContent, putRejected.StatusCode);

        var issueRejected = await session.Send(HttpMethod.Post, $"{rejectedRoute}/issue");
        Assert.True(issueRejected.IsSuccessStatusCode);

        var state = await session.Json(HttpMethod.Get, rejectedRoute);
        var number = state.GetProperty("documents")[0].GetProperty("number").GetInt64();
        Assert.Equal("Rejected", state.GetProperty("status").GetString());

        var inutPayload = new
        {
            series = 1,
            year = DateTime.UtcNow.Year,
            firstNumber = number,
            lastNumber = number,
            reason = "Numeração rejeitada para teste de recuperação"
        };
        var pending = await session.Json(HttpMethod.Post, "/api/fiscal/nfe/inutilizations", inutPayload);
        Assert.Equal("Pending", pending.GetProperty("state").GetString());

        var syncRoute = $"/api/fiscal/nfe/inutilizations/{pending.GetProperty("id").GetGuid()}/sync";
        var otherTenant = await session.Provision("Fiscal Beta", "fiscal-beta", "beta-fiscal@example.test", "Oficina2026");

        using var otherClient = host.CreateClient(new() { HandleCookies = false });
        var otherSession = new TenantTestSession(otherClient);
        await otherSession.Login("beta-fiscal@example.test");
        await otherSession.Select(otherTenant.GetProperty("id").GetGuid(), true);

        Assert.Equal(HttpStatusCode.NotFound, (await otherSession.Send(HttpMethod.Post, syncRoute)).StatusCode);
        Assert.Empty((await otherSession.Json(HttpMethod.Get, "/api/fiscal/nfe/inutilizations")).EnumerateArray());

        Assert.Equal(HttpStatusCode.Conflict, (await session.Send(HttpMethod.Post, $"{rejectedRoute}/issue")).StatusCode);

        var recovery = client.PostAsJsonAsync(syncRoute, new { });
        await gateway.RecoveryStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        try
        {
            Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync(syncRoute, new { })).StatusCode);
        }
        finally
        {
            gateway.ReleaseRecovery.TrySetResult();
        }

        Assert.True((await recovery).IsSuccessStatusCode);
        Assert.Equal("Confirmed", (await session.Json(HttpMethod.Get, "/api/fiscal/nfe/inutilizations"))[0].GetProperty("state").GetString());
        Assert.Equal("Inutilized", (await session.Json(HttpMethod.Get, rejectedRoute)).GetProperty("documents")[0].GetProperty("state").GetString());
        Assert.Equal(HttpStatusCode.NoContent, (await session.Send(HttpMethod.Post, syncRoute)).StatusCode);
        Assert.Equal(2, gateway.InutilizationPayloads.Count);
        Assert.Single(gateway.InutilizationPayloads.Distinct());
    }

    private static X509Certificate2 Certificate()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=Fiscal Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var writer = new AsnWriter(AsnEncodingRules.DER);
        var tag = new Asn1Tag(TagClass.ContextSpecific, 0, true);

        writer.PushSequence();
        writer.PushSequence(tag);
        writer.WriteObjectIdentifier("2.16.76.1.3.3");
        writer.PushSequence(tag);
        writer.WriteCharacterString(UniversalTagNumber.UTF8String, "11222333000181");
        writer.PopSequence(tag);
        writer.PopSequence(tag);
        writer.PopSequence();

        request.CertificateExtensions.Add(new X509Extension("2.5.29.17", writer.Encode(), false));
        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
    }

    // This replaces only the official network boundary; HTTP, XML signing/XSD and PostgreSQL remain real.
    private sealed class TimeoutGateway : IFiscalGateway
    {
        public FiscalOrigin Origin => FiscalOrigin.Simulation;
        public int ProductSends { get; private set; }
        public int ServiceSends { get; private set; }
        public List<string> ServicePayloads { get; } = [];
        public bool RejectProducts { get; set; }
        public List<string> InutilizationPayloads { get; } = [];
        public TaskCompletionSource RecoveryStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReleaseRecovery { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<FiscalGatewayResult> Send(FiscalDocument d, X509Certificate2 certificate, CancellationToken ct)
        {
            if (d.Kind == FiscalKind.Nfe)
            {
                ProductSends++;
                if (RejectProducts)
                {
                    return Task.FromResult(new FiscalGatewayResult(FiscalState.Rejected));
                }
            }
            else
            {
                ServiceSends++;
                ServicePayloads.Add(d.SubmittedXml!);
                if (ServiceSends == 1)
                {
                    throw new HttpRequestException("Injected timeout after sending");
                }
            }

            return Task.FromResult(new FiscalGatewayResult(
                FiscalState.Authorized,
                Key: d.Identity,
                Protocol: "TEST",
                AuthorizedXml: d.SubmittedXml));
        }

        public Task<FiscalGatewayResult> Query(FiscalDocument d, X509Certificate2 certificate, CancellationToken ct)
        {
            return Task.FromResult(new FiscalGatewayResult(FiscalState.AwaitingConfirmation, NotFound: true));
        }

        public Task<FiscalGatewayResult> Cancel(FiscalDocument d, string signedEvent, X509Certificate2 certificate, CancellationToken ct)
        {
            throw new NotSupportedException();
        }

        public async Task<FiscalGatewayResult> Inutilize(FiscalSnapshot snapshot, string signedXml, X509Certificate2 certificate, CancellationToken ct)
        {
            InutilizationPayloads.Add(signedXml);
            if (InutilizationPayloads.Count == 1)
            {
                throw new HttpRequestException("Injected timeout after inutilization");
            }

            RecoveryStarted.TrySetResult();
            await ReleaseRecovery.Task.WaitAsync(ct);

            var ns = FiscalXml.Nfe;
            var request = FiscalXml.Parse(signedXml).Descendants(ns + "infInut").Single();
            var response = new XElement(ns + "retInutNFe",
                new XElement(ns + "infInut",
                    request.Elements().Where(x => x.Name.LocalName is not ("xServ" or "xJust")),
                    new XElement(ns + "cStat", "563"),
                    new XElement(ns + "nProt", "135260000000001")));

            return NationalFiscalGateway.ParseNfe(response.ToString(), new FiscalDocument { SubmittedXml = signedXml }, "void");
        }
    }
}
