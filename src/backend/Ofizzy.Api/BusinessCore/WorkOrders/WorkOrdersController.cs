using Ofizzy.Api.Modules.Customers;
using Ofizzy.Api.Modules.Tenancy;
using Ofizzy.Api.Verticals.Automotive;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ofizzy.Api.Infrastructure.Errors;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Shared;

namespace Ofizzy.Api.Modules.WorkOrders;

[TenantAccess(Modules = new[] { ProductModule.WorkOrders, ProductModule.Customers, ProductModule.Catalog })]
[Authorize, ApiController, Route("api/work-orders")]
public sealed class WorkOrdersController(
    ApplicationDbContext db,
    CurrentTenant current,
    IValidator<WorkOrderRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<WorkOrderSummaryResponse>>> List(
        string? q = null,
        WorkOrderStatus? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.WorkOrders.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = $"%{q.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.CustomerName, term) ||
                (x.VehiclePlate != null && EF.Functions.ILike(x.VehiclePlate, term)) ||
                x.Number.ToString().Contains(q.Trim()));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.Number)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new WorkOrderSummaryResponse(
                x.Id,
                x.Number,
                x.CustomerName,
                x.VehiclePlate,
                x.VehicleDescription,
                x.Status,
                x.Services.Sum(i => i.Quantity * i.UnitPrice) + x.Parts.Sum(i => i.Quantity * i.UnitPrice),
                x.CreatedAt))
            .ToListAsync(ct);

        return Ok(new PagedResponse<WorkOrderSummaryResponse>(items, page, pageSize, total));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkOrderResponse>> Get(Guid id, CancellationToken ct)
    {
        var item = await LoadResponse(id, ct);
        return item is not null ? Ok(item) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<WorkOrderResponse>> Create(WorkOrderRequest request, CancellationToken ct)
    {
        if (!await Validate(request, ct))
        {
            return ValidationProblem(ModelState);
        }

        var automotiveEnabled = current.HasModule(ProductModule.Automotive);
        var (customer, vehicle) = await GetReferences(request, automotiveEnabled, ct);

        var item = new WorkOrder
        {
            CustomerId = customer.Id,
            VehicleId = vehicle?.Id,
            CustomerName = customer.Name,
            CustomerDocument = customer.Document,
            CustomerPhone = customer.Phone ?? customer.WhatsApp,
            VehiclePlate = vehicle?.Plate,
            VehicleDescription = VehicleDescription(vehicle),
            Mileage = request.Mileage,
            Complaint = Clean(request.Complaint),
            Diagnosis = Clean(request.Diagnosis),
            Notes = Clean(request.Notes)
        };

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var tenantId = db.TenantId!.Value;

        // UPDATE locks the tenant counter until the OS and its lines commit.
        var numbers = await db.Database.SqlQuery<long>(
            $"""
            UPDATE ofizzy.companies
            SET "LastWorkOrderNumber" = "LastWorkOrderNumber" + 1
            WHERE "TenantId" = {tenantId}
            RETURNING "LastWorkOrderNumber" AS "Value"
            """)
            .ToListAsync(ct);
        item.Number = numbers.Single();

        db.WorkOrders.Add(item);
        await ReplaceLines(item, request, ct);
        await db.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);
        return CreatedAtAction(nameof(Get), new { item.Id }, await LoadResponse(item.Id, ct));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WorkOrderResponse>> Update(Guid id, WorkOrderRequest request, CancellationToken ct)
    {
        if (!await Validate(request, ct))
        {
            return ValidationProblem(ModelState);
        }

        var item = await db.WorkOrders
            .Include(x => x.Services)
            .Include(x => x.Parts)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        if (item is null)
        {
            return NotFound();
        }

        EnsureEditable(item);

        var automotiveEnabled = current.HasModule(ProductModule.Automotive);
        var (customer, vehicle) = await GetReferences(request, automotiveEnabled, ct);
        item.CustomerId = customer.Id;
        item.CustomerName = customer.Name;
        item.CustomerDocument = customer.Document;
        item.CustomerPhone = customer.Phone ?? customer.WhatsApp;
        if (automotiveEnabled)
        {
            item.VehicleId = vehicle!.Id;
            item.VehiclePlate = vehicle.Plate;
            item.VehicleDescription = VehicleDescription(vehicle);
        }
        item.Mileage = request.Mileage;
        item.Complaint = Clean(request.Complaint);
        item.Diagnosis = Clean(request.Diagnosis);
        item.Notes = Clean(request.Notes);
        item.UpdatedAt = DateTimeOffset.UtcNow;

        db.WorkOrderServices.RemoveRange(item.Services);
        db.WorkOrderParts.RemoveRange(item.Parts);
        await ReplaceLines(item, request, ct);
        await db.SaveChangesAsync(ct);

        return Ok(await LoadResponse(id, ct));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<WorkOrderResponse>> ChangeStatus(Guid id, WorkOrderStatusRequest request, CancellationToken ct)
    {
        var item = await db.WorkOrders
            .Include(x => x.Services)
            .Include(x => x.Parts)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        if (item is null)
        {
            return NotFound();
        }

        var allowed = item.Status switch
        {
            WorkOrderStatus.Open => request.Status is WorkOrderStatus.InProgress or WorkOrderStatus.Cancelled,
            WorkOrderStatus.InProgress => request.Status is WorkOrderStatus.Open or WorkOrderStatus.Completed or WorkOrderStatus.Cancelled,
            _ => false
        };

        if (!allowed)
        {
            throw new ConflictException("A transição de estado solicitada não é permitida.");
        }

        if (request.Status == WorkOrderStatus.Completed && item.Services.Count + item.Parts.Count == 0)
        {
            throw new ConflictException("Adicione ao menos um serviço ou peça antes de finalizar a OS.");
        }

        item.Status = request.Status;
        item.CompletedAt = request.Status == WorkOrderStatus.Completed ? DateTimeOffset.UtcNow : null;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(await LoadResponse(id, ct));
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> DownloadPdf(Guid id, CancellationToken ct)
    {
        var order = await db.WorkOrders
            .AsNoTracking()
            .Include(x => x.Services)
            .Include(x => x.Parts)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        if (order is null) return NotFound();

        var company = await db.TenantSettings.AsNoTracking().SingleAsync(ct);

        var doc = new WorkOrderPdfDocument(order, company);
        var bytes = QuestPDF.Fluent.GenerateExtensions.GeneratePdf(doc);

        return File(bytes, "application/pdf", $"OS-{order.Number:D4}.pdf");
    }

    private async Task<bool> Validate(WorkOrderRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (result.IsValid)
        {
            return true;
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        return false;
    }

    private async Task<(Customer Customer, Vehicle? Vehicle)> GetReferences(
        WorkOrderRequest request,
        bool automotiveEnabled,
        CancellationToken ct)
    {
        var customer = await db.Customers
            .SingleOrDefaultAsync(x => x.Id == request.CustomerId && x.IsActive, ct)
            ?? throw new ConflictException("O cliente informado não está disponível.");

        if (!automotiveEnabled)
        {
            if (request.VehicleId.HasValue)
            {
                throw new ConflictException("O módulo de Veículos está desativado para esta organização.");
            }

            return (customer, null);
        }

        if (!request.VehicleId.HasValue)
        {
            throw new ConflictException("Selecione o veículo atendido antes de abrir a OS.");
        }

        var vehicle = await db.Vehicles
            .SingleOrDefaultAsync(x => x.Id == request.VehicleId && x.CustomerId == customer.Id && x.IsActive, ct)
            ?? throw new ConflictException("O veículo informado não pertence ao cliente ou está arquivado.");

        return (customer, vehicle);
    }

    private static string? VehicleDescription(Vehicle? vehicle) => vehicle is null
        ? null
        : string.Join(' ', new[] { vehicle.Brand, vehicle.Model, vehicle.Year?.ToString() }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

    private static void EnsureEditable(WorkOrder item)
    {
        if (item.Status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled)
        {
            throw new ConflictException("Uma OS finalizada ou cancelada não pode ser alterada.");
        }
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task ReplaceLines(WorkOrder item, WorkOrderRequest request, CancellationToken ct)
    {
        var serviceIds = request.Services
            .Where(x => x.CatalogId.HasValue)
            .Select(x => x.CatalogId!.Value)
            .Distinct()
            .ToList();

        var validServiceIds = await db.Services
            .Where(x => serviceIds.Contains(x.Id) && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(ct);

        var partIds = request.Parts
            .Where(x => x.CatalogId.HasValue)
            .Select(x => x.CatalogId!.Value)
            .Distinct()
            .ToList();

        var validParts = await db.Parts
            .Where(x => partIds.Contains(x.Id) && x.IsActive)
            .ToDictionaryAsync(x => x.Id, ct);

        if (validServiceIds.Count != serviceIds.Count || validParts.Count != partIds.Count)
        {
            throw new ConflictException("Um serviço ou peça selecionado não está mais disponível.");
        }

        foreach (var s in request.Services)
        {
            db.WorkOrderServices.Add(new WorkOrderService
            {
                WorkOrderId = item.Id,
                ServiceId = s.CatalogId,
                Description = s.Description.Trim(),
                Quantity = s.Quantity,
                UnitPrice = s.UnitPrice
            });
        }

        foreach (var p in request.Parts)
        {
            var code = !string.IsNullOrWhiteSpace(p.Code)
                ? p.Code.Trim()
                : (p.CatalogId.HasValue ? validParts[p.CatalogId.Value].Code : null);

            db.WorkOrderParts.Add(new WorkOrderPart
            {
                WorkOrderId = item.Id,
                PartId = p.CatalogId,
                Description = p.Description.Trim(),
                Code = code,
                Quantity = p.Quantity,
                UnitPrice = p.UnitPrice
            });
        }
    }

    private async Task<WorkOrderResponse?> LoadResponse(Guid id, CancellationToken ct)
    {
        var x = await db.WorkOrders
            .AsNoTracking()
            .Include(o => o.Services).ThenInclude(i => i.Service)
            .Include(o => o.Parts).ThenInclude(i => i.Part)
            .SingleOrDefaultAsync(o => o.Id == id, ct);

        if (x is null)
        {
            return null;
        }

        var services = x.Services
            .Select(i => new WorkOrderLineResponse(
                i.Id,
                i.ServiceId,
                i.Description,
                null,
                i.Quantity,
                i.UnitPrice,
                i.Quantity * i.UnitPrice))
            .ToList();

        var parts = x.Parts
            .Select(i => new WorkOrderLineResponse(
                i.Id,
                i.PartId,
                i.Description,
                i.Code ?? i.Part?.Code,
                i.Quantity,
                i.UnitPrice,
                i.Quantity * i.UnitPrice))
            .ToList();

        var servicesTotal = services.Sum(i => i.Total);
        var partsTotal = parts.Sum(i => i.Total);

        return new WorkOrderResponse(
            x.Id,
            x.Number,
            x.CustomerId,
            x.VehicleId,
            x.CustomerName,
            x.CustomerDocument,
            x.CustomerPhone,
            x.VehiclePlate,
            x.VehicleDescription,
            x.Mileage,
            x.Complaint,
            x.Diagnosis,
            x.Notes,
            x.Status,
            servicesTotal,
            partsTotal,
            servicesTotal + partsTotal,
            x.CreatedAt,
            x.CompletedAt,
            services,
            parts);
    }
}
