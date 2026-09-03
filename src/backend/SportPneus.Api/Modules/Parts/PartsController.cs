using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportPneus.Api.Infrastructure.Errors;
using SportPneus.Api.Infrastructure.Persistence;
using SportPneus.Api.Shared;
using SportPneus.Api.Shared.Validation;

namespace SportPneus.Api.Modules.Parts;

[Authorize, ApiController, Route("api/parts")]
public sealed class PartsController(ApplicationDbContext db, IValidator<PartRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<PartResponse>>> List(string? q = null, int page = 1, int pageSize = 20, bool includeArchived = false, CancellationToken ct = default)
    {
        page = Math.Max(page, 1); pageSize = Math.Clamp(pageSize, 1, 100); var query = db.Parts.AsNoTracking().Where(x => includeArchived || x.IsActive);
        if (!string.IsNullOrWhiteSpace(q)) { var term = $"%{q.Trim()}%"; var code = TextNormalization.Code(q); query = query.Where(x => EF.Functions.ILike(x.Name, term) || x.Code.Contains(code)); }
        var total = await query.CountAsync(ct); var items = await query.OrderBy(x => x.Name).Skip((page - 1) * pageSize).Take(pageSize).Select(x => new PartResponse(x.Id, x.Name, x.Code, x.CostPrice, x.SalePrice, x.IsActive)).ToListAsync(ct);
        return Ok(new PagedResponse<PartResponse>(items, page, pageSize, total));
    }
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PartResponse>> Get(Guid id, CancellationToken ct) => await db.Parts.AsNoTracking().Where(x => x.Id == id).Select(x => new PartResponse(x.Id, x.Name, x.Code, x.CostPrice, x.SalePrice, x.IsActive)).SingleOrDefaultAsync(ct) is { } item ? Ok(item) : NotFound();
    [HttpPost]
    public async Task<ActionResult<PartResponse>> Create(PartRequest request, CancellationToken ct) { var errors = await validator.ValidateAsync(request, ct); if (!errors.IsValid) return ValidationProblem(new ValidationProblemDetails(errors.ToDictionary())); var code = TextNormalization.Code(request.Code); await EnsureCode(code, null, ct); var item = new Part { Name = TextNormalization.Required(request.Name), Code = code, CostPrice = request.CostPrice, SalePrice = request.SalePrice }; db.Parts.Add(item); await db.SaveChangesAsync(ct); return CreatedAtAction(nameof(Get), new { item.Id }, Map(item)); }
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PartResponse>> Update(Guid id, PartRequest request, CancellationToken ct) { var errors = await validator.ValidateAsync(request, ct); if (!errors.IsValid) return ValidationProblem(new ValidationProblemDetails(errors.ToDictionary())); var item = await db.Parts.SingleOrDefaultAsync(x => x.Id == id, ct); if (item is null) return NotFound(); var code = TextNormalization.Code(request.Code); await EnsureCode(code, id, ct); item.Name = TextNormalization.Required(request.Name); item.Code = code; item.CostPrice = request.CostPrice; item.SalePrice = request.SalePrice; item.UpdatedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); return Ok(Map(item)); }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct) { var item = await db.Parts.SingleOrDefaultAsync(x => x.Id == id, ct); if (item is null) return NotFound(); item.IsActive = false; item.UpdatedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); return NoContent(); }
    [HttpPatch("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct) { var item = await db.Parts.SingleOrDefaultAsync(x => x.Id == id, ct); if (item is null) return NotFound(); item.IsActive = true; item.UpdatedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); return NoContent(); }
    private async Task EnsureCode(string code, Guid? id, CancellationToken ct) { if (await db.Parts.AnyAsync(x => x.Code == code && x.Id != id, ct)) throw new ConflictException("Já existe uma peça com este código."); }
    private static PartResponse Map(Part x) => new(x.Id, x.Name, x.Code, x.CostPrice, x.SalePrice, x.IsActive);
}
