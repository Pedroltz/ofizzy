using System.IO.Compression;
using System.Text;
using Ofizzy.Api.Infrastructure.Errors;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Modules.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ofizzy.Api.Modules.Fiscal;

[Authorize]
[ApiController]
[TenantAccess(Modules = new[]
{
    ProductModule.WorkOrders,
    ProductModule.Customers,
    ProductModule.Catalog,
    ProductModule.Automotive
})]
public sealed class FiscalController(
    ApplicationDbContext db,
    FiscalPreparationService preparations,
    FiscalEmissionService emissions) : ControllerBase
{
    [HttpGet("api/work-orders/{id:guid}/fiscal")]
    public async Task<ActionResult<FiscalOrderResponse>> Get(Guid id, CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";

        var prepared = await preparations.Prepare(id, ct);
        if (prepared == null)
        {
            return NotFound();
        }

        var docs = await db.FiscalDocumentEntries
            .AsNoTracking()
            .Where(x => x.WorkOrderId == id)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);

        var expected = (prepared.Order.Services.Count > 0 ? 1 : 0) + (prepared.Order.Parts.Count > 0 ? 1 : 0);
        var current = docs.Where(x => x.Environment == prepared.Snapshot.Issuer.Environment).ToList();

        var servicesTotal = FiscalValidation.Money(prepared.Order.Services.Sum(x => x.Quantity * x.UnitPrice));
        var productsTotal = prepared.Order.Parts.Sum(x => FiscalValidation.Money(x.Quantity * x.UnitPrice));
        var status = FiscalPreparationService.Status(current, expected);
        var mappedDocs = docs.Select(FiscalPreparationService.Map).ToList();

        return new FiscalOrderResponse(
            prepared.Snapshot.Recipient,
            prepared.Issues,
            servicesTotal,
            productsTotal,
            status,
            mappedDocs);
    }

    [HttpPut("api/work-orders/{id:guid}/fiscal")]
    [RequestSizeLimit(256_000)]
    public async Task<IActionResult> Save(Guid id, FiscalPreparationData request, CancellationToken ct)
    {
        var order = await db.WorkOrders
            .AsNoTracking()
            .Include(x => x.Parts)
            .Include(x => x.Services)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        if (order == null)
        {
            return NotFound();
        }

        if (request.Name?.Length > 60 || request.Document?.Length > 14 || request.Products?.Count > 100 || request.Services?.Count > 100)
        {
            throw new ConflictException("Revise os limites dos dados fiscais.");
        }

        if (request.Products?.Keys.Except(order.Parts.Select(x => x.Id)).Any() == true ||
            request.Services?.Keys.Except(order.Services.Select(x => x.Id)).Any() == true)
        {
            throw new ConflictException("Os complementos devem pertencer às linhas desta OS.");
        }

        var hasInFlightDocuments = await db.FiscalDocumentEntries.AnyAsync(
            x => x.WorkOrderId == id && (
                x.State == FiscalState.Processing ||
                x.State == FiscalState.AwaitingConfirmation ||
                x.State == FiscalState.CancellationPending),
            ct);

        if (hasInFlightDocuments)
        {
            throw new ConflictException("Aguarde a confirmação da tentativa atual antes de alterar a preparação.");
        }

        var entity = await db.FiscalPreparationEntries.SingleOrDefaultAsync(x => x.WorkOrderId == id, ct);
        if (entity == null)
        {
            entity = new FiscalPreparation { WorkOrderId = id };
            db.FiscalPreparationEntries.Add(entity);
        }

        var sanitizedServices = request.Services?.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value with
            {
                NationalCode = kvp.Value.NationalCode?.Trim() ?? string.Empty,
                MunicipalCode = string.IsNullOrWhiteSpace(kvp.Value.MunicipalCode) ? null : kvp.Value.MunicipalCode.Trim(),
                Nbs = string.IsNullOrWhiteSpace(kvp.Value.Nbs) ? null : kvp.Value.Nbs.Trim()
            });

        var sanitized = request with { Services = sanitizedServices };
        entity.Data = FiscalJson.Write(sanitized);
        entity.Version = Guid.NewGuid();

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("api/work-orders/{id:guid}/fiscal/issue")]
    public async Task<IActionResult> Issue(Guid id, CancellationToken ct)
    {
        if (!await db.WorkOrders.AnyAsync(x => x.Id == id, ct))
        {
            return NotFound();
        }

        await emissions.Issue(id, ct);
        return Accepted($"/api/work-orders/{id}/fiscal");
    }

    [HttpGet("api/fiscal/documents/{id:guid}")]
    public async Task<ActionResult<FiscalDocumentResponse>> Document(Guid id, CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";

        var doc = await db.FiscalDocumentEntries
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        return doc == null ? NotFound() : FiscalPreparationService.Map(doc);
    }

    [HttpPost("api/fiscal/documents/{id:guid}/sync")]
    public async Task<IActionResult> Sync(Guid id, CancellationToken ct)
    {
        if (!await db.FiscalDocumentEntries.AnyAsync(x => x.Id == id, ct))
        {
            return NotFound();
        }

        await emissions.Process(id, true, null, ct);
        return NoContent();
    }

    [HttpPost("api/fiscal/documents/{id:guid}/cancel")]
    [TenantAccess(Admin = true)]
    public async Task<IActionResult> Cancel(Guid id, CancelFiscalRequest request, CancellationToken ct)
    {
        if (!await db.FiscalDocumentEntries.AnyAsync(x => x.Id == id, ct))
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length is < 15 or > 255)
        {
            throw new ConflictException("Informe justificativa de 15 a 255 caracteres.");
        }

        await emissions.Process(id, false, request.Reason.Trim(), ct);
        return Accepted($"/api/fiscal/documents/{id}");
    }

    [HttpGet("api/fiscal/documents/{id:guid}/xml")]
    public async Task<IActionResult> Xml(Guid id, CancellationToken ct)
    {
        var doc = await db.FiscalDocumentEntries
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        if (doc == null)
        {
            return NotFound();
        }

        if (doc.AuthorizedXml == null)
        {
            throw new ConflictException("O XML fica disponível após autorização fiscal.");
        }

        Response.Headers.CacheControl = "no-store";
        return File(Encoding.UTF8.GetBytes(doc.AuthorizedXml), "application/xml", $"{FileName(doc)}.xml");
    }

    [HttpGet("api/fiscal/documents/{id:guid}/pdf")]
    public async Task<IActionResult> Pdf(Guid id, CancellationToken ct)
    {
        var doc = await db.FiscalDocumentEntries
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        if (doc == null)
        {
            return NotFound();
        }

        if (doc.AuthorizedXml == null)
        {
            throw new ConflictException("O PDF fica disponível após autorização fiscal.");
        }

        Response.Headers.CacheControl = "no-store";
        return File(FiscalPdf.Generate(doc), "application/pdf", $"{FileName(doc)}.pdf");
    }

    [HttpGet("api/work-orders/{id:guid}/fiscal/download")]
    public async Task<IActionResult> Download(Guid id, bool includePdf = false, CancellationToken ct = default)
    {
        var order = await db.WorkOrders
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        if (order == null)
        {
            return NotFound();
        }

        var settings = await db.FiscalSettingsEntries
            .AsNoTracking()
            .SingleOrDefaultAsync(ct);

        var environment = settings == null
            ? FiscalEnvironment.Homologation
            : FiscalJson.Required<FiscalSettingsData>(settings.Data).Environment;

        var docs = await db.FiscalDocumentEntries
            .AsNoTracking()
            .Where(x => x.WorkOrderId == id && x.Environment == environment && x.State == FiscalState.Authorized && x.AuthorizedXml != null)
            .ToListAsync(ct);

        if (docs.Count == 0)
        {
            throw new ConflictException("Ainda não há documentos autorizados para baixar.");
        }

        var expected = (await db.WorkOrderParts.AnyAsync(x => x.WorkOrderId == id, ct) ? 1 : 0)
            + (await db.WorkOrderServices.AnyAsync(x => x.WorkOrderId == id, ct) ? 1 : 0);

        using var buffer = new MemoryStream();
        using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, true))
        {
            foreach (var doc in docs)
            {
                var xmlEntry = zip.CreateEntry($"{FileName(doc)}.xml");
                using (var stream = xmlEntry.Open())
                {
                    stream.Write(Encoding.UTF8.GetBytes(doc.AuthorizedXml!));
                }

                if (includePdf)
                {
                    var pdfEntry = zip.CreateEntry($"{FileName(doc)}.pdf");
                    using (var stream = pdfEntry.Open())
                    {
                        stream.Write(FiscalPdf.Generate(doc));
                    }
                }
            }
        }

        Response.Headers.CacheControl = "no-store";
        var zipSuffix = docs.Count < expected ? "-parcial" : string.Empty;
        return File(buffer.ToArray(), "application/zip", $"OS-{order.Number:D4}-fiscal{zipSuffix}.zip");
    }

    private static string FileName(FiscalDocument doc) => $"{doc.Kind}-{doc.Environment}-{doc.Id:N}";
}
