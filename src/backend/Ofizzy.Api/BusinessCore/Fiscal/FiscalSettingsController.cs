using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ofizzy.Api.Infrastructure.Errors;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Modules.Tenancy;

namespace Ofizzy.Api.Modules.Fiscal;

[Authorize, ApiController, Route("api/fiscal"), TenantAccess(Admin = true)]
public sealed class FiscalSettingsController(ApplicationDbContext db, FiscalCertificateVault vault, IConfiguration configuration, IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet("settings")]
    public async Task<ActionResult<FiscalSettingsResponse>> Get(CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        var entity = await db.FiscalSettingsEntries.AsNoTracking().SingleOrDefaultAsync(ct);
        FiscalSettingsData data;
        if (entity == null)
        {
            var company = await db.TenantSettings.AsNoTracking().SingleAsync(ct);
            data = new(Cnpj: company.Cnpj ?? "", LegalName: company.LegalName ?? company.Name,
                Address: new(Street: company.Address ?? "", City: company.City ?? "", State: company.State ?? "", PostalCode: company.PostalCode ?? ""));
        }
        else data = FiscalJson.Required<FiscalSettingsData>(entity.Data);
        return new FiscalSettingsResponse(data, entity?.CertificateExpiresAt is {} expires ? new(entity.CertificateSubject!, expires, entity.CertificateThumbprint!) : null,
            vault.Configured, configuration.GetValue<bool>("Fiscal:ProductionEnabled"),
            FiscalReleaseGate.DevToolsAvailable(environment, configuration, data.Environment));
    }
    [HttpPut("settings")]
    public async Task<IActionResult> Save(FiscalSettingsData request, CancellationToken ct)
    {
        var result = new FiscalSettingsValidator().Validate(request);
        if (!result.IsValid) return ValidationProblem(new ValidationProblemDetails(result.ToDictionary()));
        if (request.Environment == FiscalEnvironment.Production && !configuration.GetValue<bool>("Fiscal:ProductionEnabled"))
            throw new ConflictException("Produção fiscal ainda não foi liberada no servidor após homologação.");
        var entity = await db.FiscalSettingsEntries.SingleOrDefaultAsync(ct);
        if (entity == null) { entity = new(); db.FiscalSettingsEntries.Add(entity); }
        else
        {
            var previous = FiscalJson.Required<FiscalSettingsData>(entity.Data);
            if (previous.Cnpj != request.Cnpj && (entity.Certificate != null || await db.FiscalDocumentEntries.AnyAsync(ct)))
                throw new ConflictException("O CNPJ está vinculado ao certificado ou histórico fiscal e não pode ser alterado.");
        }
        entity.Data = FiscalJson.Write(request); entity.Version = Guid.NewGuid();
        await db.SaveChangesAsync(ct); return NoContent();
    }
    [HttpPost("certificate"), RequestSizeLimit(1_100_000)]
    public async Task<IActionResult> Certificate([FromForm] IFormFile file, [FromForm] string password, CancellationToken ct)
    {
        if (file.Length is <= 0 or > 1_000_000 || password.Length > 1024) throw new ConflictException("Envie um certificado A1 de até 1 MB e sua senha.");
        var entity = await db.FiscalSettingsEntries.SingleOrDefaultAsync(ct) ?? throw new ConflictException("Salve os dados fiscais antes do certificado.");
        var data = FiscalJson.Required<FiscalSettingsData>(entity.Data);
        using var buffer = new MemoryStream(); await file.CopyToAsync(buffer, ct); var bytes = buffer.ToArray();
        byte[]? exported = null;
        try
        {
            using var cert = X509CertificateLoader.LoadPkcs12(bytes, password, X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);
            if (!cert.HasPrivateKey || cert.GetRSAPrivateKey() is not {} rsa) throw new ConflictException("O A1 deve conter chave privada RSA.");
            rsa.Dispose();
            if (cert.NotAfter.ToUniversalTime() <= DateTime.UtcNow || cert.NotBefore.ToUniversalTime() > DateTime.UtcNow) throw new ConflictException("Certificado fora da validade.");
            if (!FiscalCertificateVault.MatchesCnpj(cert, data.Cnpj)) throw new ConflictException("O CNPJ do certificado deve corresponder ao CNPJ configurado.");
            exported = cert.Export(X509ContentType.Pkcs12);
            entity.Certificate = vault.Protect(entity.TenantId, exported); entity.KeyId = vault.ActiveKeyId;
            entity.CertificateExpiresAt = new DateTimeOffset(cert.NotAfter.ToUniversalTime());
            entity.CertificateSubject = cert.GetNameInfo(X509NameType.SimpleName, false);
            entity.CertificateThumbprint = cert.Thumbprint; entity.Version = Guid.NewGuid();
            await db.SaveChangesAsync(ct); return NoContent();
        }
        catch (CryptographicException) { throw new ConflictException("Certificado inválido ou senha incorreta."); }
        finally { CryptographicOperations.ZeroMemory(bytes); if (exported != null) CryptographicOperations.ZeroMemory(exported); if (buffer.TryGetBuffer(out var segment)) CryptographicOperations.ZeroMemory(segment.AsSpan()); }
    }
}

[Authorize, ApiController, TenantAccess(Modules = new[] { ProductModule.Catalog })]
public sealed class FiscalCatalogController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("api/parts/{id:guid}/fiscal")]
    public async Task<ActionResult<ProductFiscalData>> Product(Guid id, CancellationToken ct)
    {
        if (!await db.Parts.AnyAsync(x => x.Id == id, ct)) return NotFound();
        var x = await db.ProductFiscalProfileEntries.AsNoTracking().SingleOrDefaultAsync(x => x.PartId == id, ct);
        return x == null ? new ProductFiscalData() : FiscalJson.Required<ProductFiscalData>(x.Data);
    }
    [HttpPut("api/parts/{id:guid}/fiscal"), TenantAccess(Admin = true)]
    public async Task<IActionResult> Product(Guid id, ProductFiscalData request, CancellationToken ct)
    {
        if (!await db.Parts.AnyAsync(x => x.Id == id, ct)) return NotFound();
        var validation = new ProductFiscalValidator().Validate(request);
        if (!validation.IsValid) return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        var entity = await db.ProductFiscalProfileEntries.SingleOrDefaultAsync(x => x.PartId == id, ct);
        if (entity == null) { entity = new() { PartId = id }; db.ProductFiscalProfileEntries.Add(entity); }
        entity.Data = FiscalJson.Write(request); await db.SaveChangesAsync(ct); return NoContent();
    }
    [HttpGet("api/services/{id:guid}/fiscal")]
    public async Task<ActionResult<ServiceFiscalData>> Service(Guid id, CancellationToken ct)
    {
        if (!await db.Services.AnyAsync(x => x.Id == id, ct)) return NotFound();
        var x = await db.ServiceFiscalProfileEntries.AsNoTracking().SingleOrDefaultAsync(x => x.ServiceId == id, ct);
        return x == null ? new ServiceFiscalData() : FiscalJson.Required<ServiceFiscalData>(x.Data);
    }
    [HttpPut("api/services/{id:guid}/fiscal"), TenantAccess(Admin = true)]
    public async Task<IActionResult> Service(Guid id, ServiceFiscalData request, CancellationToken ct)
    {
        if (!await db.Services.AnyAsync(x => x.Id == id, ct)) return NotFound();
        var validation = new ServiceFiscalValidator().Validate(request);
        if (!validation.IsValid) return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        var entity = await db.ServiceFiscalProfileEntries.SingleOrDefaultAsync(x => x.ServiceId == id, ct);
        if (entity == null) { entity = new() { ServiceId = id }; db.ServiceFiscalProfileEntries.Add(entity); }
        entity.Data = FiscalJson.Write(request); await db.SaveChangesAsync(ct); return NoContent();
    }
}
