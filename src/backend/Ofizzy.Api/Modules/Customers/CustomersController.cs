using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ofizzy.Api.Infrastructure.Errors;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Shared;
using Ofizzy.Api.Shared.Validation;

namespace Ofizzy.Api.Modules.Customers;

[Authorize, ApiController, Route("api/customers")]
public sealed class CustomersController(ApplicationDbContext db, IValidator<CustomerRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<CustomerResponse>>> List(string? q = null, int page = 1, int pageSize = 20, bool includeArchived = false, CancellationToken ct = default)
    {
        page = Math.Max(page, 1); pageSize = Math.Clamp(pageSize, 1, 100); var query = db.Customers.AsNoTracking().Where(x => includeArchived || x.IsActive);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = $"%{q.Trim()}%"; var digits = TextNormalization.Digits(q);
            query = digits is null
                ? query.Where(x => EF.Functions.ILike(x.Name, term))
                : query.Where(x => EF.Functions.ILike(x.Name, term) || (x.Phone != null && x.Phone.Contains(digits)) || (x.Document != null && x.Document.Contains(digits)));
        }
        var total = await query.CountAsync(ct); var items = await query.OrderBy(x => x.Name).Skip((page - 1) * pageSize).Take(pageSize).Select(Map()).ToListAsync(ct);
        return Ok(new PagedResponse<CustomerResponse>(items, page, pageSize, total));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerResponse>> Get(Guid id, CancellationToken ct) => await db.Customers.AsNoTracking().Where(x => x.Id == id).Select(Map()).SingleOrDefaultAsync(ct) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> Create(CustomerRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct); if (!validation.IsValid) return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        var document = TextNormalization.Digits(request.Document); await EnsureDocumentAvailable(document, null, ct);
        var item = new Customer { Name = TextNormalization.Required(request.Name), Document = document, Phone = TextNormalization.Optional(request.Phone), WhatsApp = TextNormalization.Optional(request.WhatsApp), Email = TextNormalization.Optional(request.Email)?.ToLowerInvariant(), Address = TextNormalization.Optional(request.Address), Notes = TextNormalization.Optional(request.Notes) };
        db.Customers.Add(item); await db.SaveChangesAsync(ct); return CreatedAtAction(nameof(Get), new { item.Id }, ToResponse(item));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerResponse>> Update(Guid id, CustomerRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct); if (!validation.IsValid) return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        var item = await db.Customers.SingleOrDefaultAsync(x => x.Id == id, ct); if (item is null) return NotFound();
        var document = TextNormalization.Digits(request.Document); await EnsureDocumentAvailable(document, id, ct);
        item.Name = TextNormalization.Required(request.Name); item.Document = document; item.Phone = TextNormalization.Optional(request.Phone); item.WhatsApp = TextNormalization.Optional(request.WhatsApp);
        item.Email = TextNormalization.Optional(request.Email)?.ToLowerInvariant(); item.Address = TextNormalization.Optional(request.Address); item.Notes = TextNormalization.Optional(request.Notes); item.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct); return Ok(ToResponse(item));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        var item = await db.Customers.Include(x => x.Vehicles).SingleOrDefaultAsync(x => x.Id == id, ct); if (item is null) return NotFound();
        item.IsActive = false; item.UpdatedAt = DateTimeOffset.UtcNow; foreach (var vehicle in item.Vehicles) { vehicle.IsActive = false; vehicle.UpdatedAt = DateTimeOffset.UtcNow; }
        await db.SaveChangesAsync(ct); return NoContent();
    }

    [HttpPatch("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct) { var item = await db.Customers.SingleOrDefaultAsync(x => x.Id == id, ct); if (item is null) return NotFound(); item.IsActive = true; item.UpdatedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); return NoContent(); }

    private async Task EnsureDocumentAvailable(string? document, Guid? currentId, CancellationToken ct) { if (document is not null && await db.Customers.AnyAsync(x => x.Document == document && x.Id != currentId, ct)) throw new ConflictException("Já existe um cliente com este CPF/CNPJ."); }
    private static System.Linq.Expressions.Expression<Func<Customer, CustomerResponse>> Map() => x => new(x.Id, x.Name, x.Document, x.Phone, x.WhatsApp, x.Email, x.Address, x.Notes, x.IsActive, x.CreatedAt);
    private static CustomerResponse ToResponse(Customer x) => new(x.Id, x.Name, x.Document, x.Phone, x.WhatsApp, x.Email, x.Address, x.Notes, x.IsActive, x.CreatedAt);
}
