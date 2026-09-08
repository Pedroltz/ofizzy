using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Shared;
using Ofizzy.Api.Shared.Validation;

namespace Ofizzy.Api.Modules.Services;

[Authorize, ApiController, Route("api/services")]
public sealed class ServicesController(ApplicationDbContext db, IValidator<ServiceRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ServiceResponse>>> List(string? q = null, int page = 1, int pageSize = 20, bool includeArchived = false, CancellationToken ct = default)
    {
        page = Math.Max(page, 1); pageSize = Math.Clamp(pageSize, 1, 100); var query = db.Services.AsNoTracking().Where(x => includeArchived || x.IsActive);
        if (!string.IsNullOrWhiteSpace(q)) { var term = $"%{q.Trim()}%"; query = query.Where(x => EF.Functions.ILike(x.Name, term) || (x.Description != null && EF.Functions.ILike(x.Description, term))); }
        var total = await query.CountAsync(ct); var items = await query.OrderBy(x => x.Name).Skip((page - 1) * pageSize).Take(pageSize).Select(x => new ServiceResponse(x.Id, x.Name, x.Description, x.DefaultPrice, x.IsActive)).ToListAsync(ct);
        return Ok(new PagedResponse<ServiceResponse>(items, page, pageSize, total));
    }
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceResponse>> Get(Guid id, CancellationToken ct) => await db.Services.AsNoTracking().Where(x => x.Id == id).Select(x => new ServiceResponse(x.Id, x.Name, x.Description, x.DefaultPrice, x.IsActive)).SingleOrDefaultAsync(ct) is { } item ? Ok(item) : NotFound();
    [HttpPost]
    public async Task<ActionResult<ServiceResponse>> Create(ServiceRequest request, CancellationToken ct) { var errors = await validator.ValidateAsync(request, ct); if (!errors.IsValid) return ValidationProblem(new ValidationProblemDetails(errors.ToDictionary())); var item = new ServiceItem { Name = TextNormalization.Required(request.Name), Description = TextNormalization.Optional(request.Description), DefaultPrice = request.DefaultPrice }; db.Services.Add(item); await db.SaveChangesAsync(ct); return CreatedAtAction(nameof(Get), new { item.Id }, Map(item)); }
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ServiceResponse>> Update(Guid id, ServiceRequest request, CancellationToken ct) { var errors = await validator.ValidateAsync(request, ct); if (!errors.IsValid) return ValidationProblem(new ValidationProblemDetails(errors.ToDictionary())); var item = await db.Services.SingleOrDefaultAsync(x => x.Id == id, ct); if (item is null) return NotFound(); item.Name = TextNormalization.Required(request.Name); item.Description = TextNormalization.Optional(request.Description); item.DefaultPrice = request.DefaultPrice; item.UpdatedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); return Ok(Map(item)); }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct) { var item = await db.Services.SingleOrDefaultAsync(x => x.Id == id, ct); if (item is null) return NotFound(); item.IsActive = false; item.UpdatedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); return NoContent(); }
    [HttpPatch("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct) { var item = await db.Services.SingleOrDefaultAsync(x => x.Id == id, ct); if (item is null) return NotFound(); item.IsActive = true; item.UpdatedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); return NoContent(); }
    private static ServiceResponse Map(ServiceItem x) => new(x.Id, x.Name, x.Description, x.DefaultPrice, x.IsActive);
}
