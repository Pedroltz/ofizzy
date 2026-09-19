using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Modules.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ofizzy.Api.Modules.Fiscal;

/// <summary>
/// Controlador utilitário exclusivo para o ambiente de desenvolvimento (Fase 8).
/// Em ambiente de produção, este controlador é desativado e retorna 404.
/// </summary>
[Authorize]
[ApiController]
[Route("api/fiscal/dev")]
[TenantAccess(Admin = true)]
public sealed class FiscalDevController(
    ApplicationDbContext db,
    IWebHostEnvironment env,
    IConfiguration config) : ControllerBase
{
    [HttpGet("certificate")]
    public async Task<IActionResult> DownloadTestCertificate(CancellationToken ct)
    {
        if (!FiscalReleaseGate.DevToolsAvailable(env, config, FiscalEnvironment.Homologation))
        {
            return NotFound();
        }

        Response.Headers.CacheControl = "no-store";

        var entity = await db.FiscalSettingsEntries.SingleOrDefaultAsync(ct);
        var cnpj = "11222333000181";

        if (entity?.Data != null)
        {
            var settings = FiscalJson.Required<FiscalSettingsData>(entity.Data);
            if (!FiscalReleaseGate.DevToolsAvailable(env, config, settings.Environment))
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(settings.Cnpj))
            {
                cnpj = settings.Cnpj;
            }
        }

        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            $"CN=Oficina Teste Dev {cnpj}",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        var writer = new AsnWriter(AsnEncodingRules.DER);
        var tag = new Asn1Tag(TagClass.ContextSpecific, 0, true);

        writer.PushSequence();
        writer.PushSequence(tag);
        writer.WriteObjectIdentifier("2.16.76.1.3.3");
        writer.PushSequence(tag);
        writer.WriteCharacterString(UniversalTagNumber.UTF8String, cnpj);
        writer.PopSequence(tag);
        writer.PopSequence(tag);
        writer.PopSequence();

        request.CertificateExtensions.Add(new X509Extension("2.5.29.17", writer.Encode(), false));

        using var cert = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(1));

        var pfx = cert.Export(X509ContentType.Pkcs12, "teste123");

        return File(pfx, "application/x-pkcs12", $"ofizzy-dev-{cnpj}.pfx");
    }
}
