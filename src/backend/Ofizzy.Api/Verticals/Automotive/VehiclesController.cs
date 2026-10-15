using Ofizzy.Api.Modules.Tenancy;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ofizzy.Api.Infrastructure.Errors;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Shared;
using Ofizzy.Api.Shared.Validation;

namespace Ofizzy.Api.Verticals.Automotive;

[TenantAccess(Modules = new[] { ProductModule.Customers, ProductModule.Automotive })]
[Authorize, ApiController, Route("api/vehicles")]
public sealed class VehiclesController(ApplicationDbContext db, IValidator<VehicleRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<VehicleResponse>>> List(string? q = null, Guid? customerId = null, int page = 1, int pageSize = 20, bool includeArchived = false, CancellationToken ct = default)
    {
        page = Math.Max(page, 1); pageSize = Math.Clamp(pageSize, 1, 100); var query = db.Vehicles.AsNoTracking().Where(x => includeArchived || x.IsActive);
        if (customerId.HasValue) query = query.Where(x => x.CustomerId == customerId);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = $"%{q.Trim()}%";
            var plate = TextNormalization.Plate(q);
            query = query.Where(x => (!string.IsNullOrEmpty(plate) && x.Plate.Contains(plate)) || EF.Functions.ILike(x.Model, term) || (x.Brand != null && EF.Functions.ILike(x.Brand, term)) || EF.Functions.ILike(x.Customer.Name, term));
        }
        var total = await query.CountAsync(ct); var items = await query.OrderBy(x => x.Plate).Skip((page - 1) * pageSize).Take(pageSize).Select(Map()).ToListAsync(ct);
        return Ok(new PagedResponse<VehicleResponse>(items, page, pageSize, total));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VehicleResponse>> Get(Guid id, CancellationToken ct)
    {
        var item = await db.Vehicles
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(Map())
            .SingleOrDefaultAsync(ct);

        return item is not null ? Ok(item) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<VehicleResponse>> Create(VehicleRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        }

        var plate = TextNormalization.Plate(request.Plate);
        await EnsureReferences(request.CustomerId, plate, null, ct);

        var item = new Vehicle
        {
            CustomerId = request.CustomerId,
            Plate = plate,
            Brand = TextNormalization.Optional(request.Brand),
            Model = TextNormalization.Required(request.Model),
            Year = request.Year,
            Color = TextNormalization.Optional(request.Color),
            Mileage = request.Mileage,
            Chassis = TextNormalization.Optional(request.Chassis),
            Notes = TextNormalization.Optional(request.Notes)
        };

        db.Vehicles.Add(item);
        await db.SaveChangesAsync(ct);

        var response = await db.Vehicles
            .AsNoTracking()
            .Where(x => x.Id == item.Id)
            .Select(Map())
            .SingleAsync(ct);

        return CreatedAtAction(nameof(Get), new { item.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VehicleResponse>> Update(Guid id, VehicleRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        }

        var item = await db.Vehicles.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var plate = TextNormalization.Plate(request.Plate);
        await EnsureReferences(request.CustomerId, plate, id, ct);

        item.CustomerId = request.CustomerId;
        item.Plate = plate;
        item.Brand = TextNormalization.Optional(request.Brand);
        item.Model = TextNormalization.Required(request.Model);
        item.Year = request.Year;
        item.Color = TextNormalization.Optional(request.Color);
        item.Mileage = request.Mileage;
        item.Chassis = TextNormalization.Optional(request.Chassis);
        item.Notes = TextNormalization.Optional(request.Notes);
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        var response = await db.Vehicles
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(Map())
            .SingleAsync(ct);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        var item = await db.Vehicles.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item is null)
        {
            return NotFound();
        }

        item.IsActive = false;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPatch("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        var item = await db.Vehicles.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item is null)
        {
            return NotFound();
        }

        if (!await db.Customers.AnyAsync(x => x.Id == item.CustomerId && x.IsActive, ct))
        {
            throw new ConflictException("Restaure o cliente antes do veículo.");
        }

        item.IsActive = true;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task EnsureReferences(Guid customerId, string plate, Guid? currentId, CancellationToken ct)
    {
        if (!await db.Customers.AnyAsync(x => x.Id == customerId && x.IsActive, ct))
        {
            throw new ConflictException("O cliente informado não está disponível.");
        }

        if (await db.Vehicles.AnyAsync(x => x.Plate == plate && x.Id != currentId, ct))
        {
            throw new ConflictException("Já existe um veículo com esta placa.");
        }
    }

    private static System.Linq.Expressions.Expression<Func<Vehicle, VehicleResponse>> Map() => x => new(
        x.Id,
        x.CustomerId,
        x.Customer.Name,
        x.Plate,
        x.Brand,
        x.Model,
        x.Year,
        x.Color,
        x.Mileage,
        x.Chassis,
        x.Notes,
        x.IsActive,
        x.CreatedAt);
}
