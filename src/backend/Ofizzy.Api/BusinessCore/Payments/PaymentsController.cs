using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ofizzy.Api.Infrastructure.Errors;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Modules.Fiscal;
using Ofizzy.Api.Modules.Tenancy;
using Ofizzy.Api.Modules.WorkOrders;

namespace Ofizzy.Api.Modules.Payments;

[Authorize]
[ApiController]
[TenantAccess(Modules = new[] { ProductModule.WorkOrders, ProductModule.Customers, ProductModule.Catalog })]
public sealed class PaymentsController(ApplicationDbContext db, CurrentTenant tenant) : ControllerBase
{
    [HttpGet("api/work-orders/{id:guid}/payments")]
    public async Task<ActionResult<OrderPaymentsResponse>> List(Guid id, CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        var order = await Order(id, ct);
        if (order == null) return NotFound();
        var payments = await db.Set<Payment>().AsNoTracking().Include(x => x.Allocations).Include(x => x.Movements)
            .Where(x => x.WorkOrderId == id).OrderBy(x => x.CreatedAt).ToListAsync(ct);
        var docs = await db.FiscalDocumentEntries.AsNoTracking().Where(x => x.WorkOrderId == id)
            .ToDictionaryAsync(x => x.Id, x => x.State, ct);
        var rows = payments.Select(p => Map(p, docs)).ToList();
        var total = Total(order);
        var settled = rows.Sum(x => x.Settled);
        var reversed = rows.Sum(x => x.Reversed);
        var officialDocs = await db.FiscalDocumentEntries.AsNoTracking().Where(x => x.WorkOrderId == id
            && x.Origin == FiscalOrigin.Official && x.Environment == FiscalEnvironment.Production && x.State == FiscalState.Authorized).ToListAsync(ct);
        var allocations = await db.Set<PaymentAllocation>().AsNoTracking()
            .Where(x => officialDocs.Select(d => d.Id).Contains(x.DocumentId)).ToListAsync(ct);
        var available = officialDocs.Select(d => new PaymentDocumentResponse(d.Id, d.Kind.ToString(), d.Total,
            d.Total - allocations.Where(a => a.DocumentId == d.Id).Sum(a => a.Amount))).ToList();
        return new OrderPaymentsResponse(total, rows.Sum(x => x.Amount), settled, reversed,
            total - settled + reversed, rows, available);
    }

    [HttpPost("api/work-orders/{id:guid}/payments")]
    public async Task<IActionResult> Register(Guid id, RegisterPaymentRequest request, CancellationToken ct)
    {
        Money(request.Amount, positive: true);
        ValidateRequest(request.RequestId, request.ReceivedAt);
        if (request.Method is not ("Cash" or "Pix" or "Transfer" or "Card" or "Cheque"))
            throw new ConflictException("Selecione uma forma de recebimento válida.");
        var allocations = request.Allocations ?? [];
        if (allocations.Count > 100 || allocations.Select(x => x.DocumentId).Distinct().Count() != allocations.Count)
            throw new ConflictException("Informe até 100 documentos distintos.");
        foreach (var allocation in allocations) Money(allocation.Amount, positive: true);
        if (allocations.Count > 0 && allocations.Sum(x => x.Amount) != request.Amount)
            throw new ConflictException("Os vínculos fiscais devem somar o valor do recebimento.");
        var hash = Hash(request with { Allocations = allocations.OrderBy(x => x.DocumentId).ToArray() });
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var previous = await db.Set<Payment>().SingleOrDefaultAsync(x => x.RequestId == request.RequestId, ct);
        if (previous != null)
        {
            EnsureSame(previous.RequestHash, hash, previous.WorkOrderId == id);
            return Ok(new { previous.Id });
        }
        var order = await Order(id, ct);
        if (order == null) return NotFound();
        if (order.Status != WorkOrderStatus.Completed)
            throw new ConflictException("Conclua a OS antes de registrar recebimentos.");
        var existing = await db.Set<Payment>().Include(x => x.Movements).Where(x => x.WorkOrderId == id).ToListAsync(ct);
        var registeredNet = existing.Sum(x => x.Amount - x.Movements.Where(m => m.Kind == "Reversal").Sum(m => m.Amount));
        if (registeredNet + request.Amount > Total(order))
            throw new ConflictException("O recebimento excede o saldo disponível da OS.");
        var payment = new Payment
        {
            WorkOrderId = id, RequestId = request.RequestId, RequestHash = hash, Amount = request.Amount,
            Method = request.Method, ReceivedAt = request.ReceivedAt.ToUniversalTime(), CreatedBy = tenant.UserId!.Value
        };
        foreach (var allocation in allocations)
        {
            var doc = await db.FiscalDocumentEntries.SingleOrDefaultAsync(x => x.Id == allocation.DocumentId && x.WorkOrderId == id, ct);
            if (doc == null || doc.State != FiscalState.Authorized || doc.Origin != FiscalOrigin.Official || doc.Environment != FiscalEnvironment.Production)
                throw new ConflictException("Vincule somente documentos oficiais autorizados em produção desta OS.");
            var alreadyAllocated = await db.Set<PaymentAllocation>().Where(x => x.DocumentId == doc.Id).SumAsync(x => x.Amount, ct);
            if (alreadyAllocated + allocation.Amount > doc.Total)
                throw new ConflictException("O vínculo excede o valor disponível do documento fiscal.");
            payment.Allocations.Add(new PaymentAllocation { DocumentId = doc.Id, Amount = allocation.Amount });
        }
        db.Add(payment);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return Created($"/api/work-orders/{id}/payments", new { payment.Id });
    }

    [HttpPost("api/payments/{id:guid}/settlements")]
    public async Task<IActionResult> Settle(Guid id, SettlePaymentRequest request, CancellationToken ct)
    {
        Money(request.Amount, true);
        Money(request.Fees, false);
        if (request.SegregatedTax is { } tax) Money(tax, false);
        ValidateRequest(request.RequestId, request.SettledAt);
        if (request.Fees + (request.SegregatedTax ?? 0) > request.Amount)
            throw new ConflictException("Taxas e tributos segregados não podem exceder o valor bruto.");
        return await AddMovement(id, request.RequestId, Hash(request), "Settlement", request.Amount, request.Fees,
            request.SegregatedTax, request.SettledAt, null, null, ct);
    }

    [HttpPost("api/payments/{id:guid}/reversals")]
    [TenantAccess(Admin = true, Modules = new[] { ProductModule.WorkOrders, ProductModule.Customers, ProductModule.Catalog })]
    public async Task<IActionResult> Reverse(Guid id, ReversePaymentRequest request, CancellationToken ct)
    {
        Money(request.Amount, true);
        ValidateRequest(request.RequestId, request.ReversedAt);
        if (request.Reason?.Trim().Length is not (>= 15 and <= 255))
            throw new ConflictException("Informe justificativa de estorno com 15 a 255 caracteres.");
        return await AddMovement(id, request.RequestId, Hash(request), "Reversal", request.Amount, 0, null,
            request.ReversedAt, request.SettlementId, request.Reason.Trim(), ct);
    }

    private async Task<IActionResult> AddMovement(Guid id, Guid requestId, string hash, string kind, decimal amount,
        decimal fees, decimal? tax, DateTimeOffset at, Guid? settlementId, string? reason, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var previous = await db.Set<PaymentMovement>().SingleOrDefaultAsync(x => x.RequestId == requestId, ct);
        if (previous != null)
        {
            EnsureSame(previous.RequestHash, hash, previous.PaymentId == id);
            return Ok(new { previous.Id });
        }
        var payment = await db.Set<Payment>().Include(x => x.Movements).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (payment == null) return NotFound();
        if (kind == "Settlement")
        {
            if (at < payment.ReceivedAt || payment.Movements.Where(x => x.Kind == "Settlement").Sum(x => x.Amount) + amount > payment.Amount)
                throw new ConflictException("A liquidação deve ocorrer após o recebimento e respeitar o valor ainda não liquidado.");
        }
        else
        {
            var settlement = payment.Movements.SingleOrDefault(x => x.Id == settlementId && x.Kind == "Settlement");
            if (settlement == null || at < settlement.OccurredAt || payment.Movements.Where(x => x.SettlementId == settlementId).Sum(x => x.Amount) + amount > settlement.Amount)
                throw new ConflictException("O estorno deve corresponder a uma liquidação, ocorrer após ela e respeitar seu saldo.");
        }
        var movement = new PaymentMovement
        {
            PaymentId = id, RequestId = requestId, RequestHash = hash, Kind = kind, Amount = amount, Fees = fees,
            SegregatedTax = tax, OccurredAt = at.ToUniversalTime(), SettlementId = settlementId, Reason = reason,
            CreatedBy = tenant.UserId!.Value
        };
        db.Add(movement);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return Created($"/api/work-orders/{payment.WorkOrderId}/payments", new { movement.Id });
    }

    private Task<WorkOrder?> Order(Guid id, CancellationToken ct) => db.WorkOrders.AsNoTracking()
        .Include(x => x.Parts).Include(x => x.Services).SingleOrDefaultAsync(x => x.Id == id, ct);
    private static decimal Total(WorkOrder order) => FiscalValidation.Money(order.Services.Sum(x => x.Quantity * x.UnitPrice)
        + order.Parts.Sum(x => x.Quantity * x.UnitPrice));
    private static string Hash<T>(T request) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request))));
    private static void EnsureSame(string stored, string actual, bool sameOwner)
    {
        if (stored != actual || !sameOwner) throw new ConflictException("Este identificador já foi usado com outros dados. Reenvie a requisição original.");
    }
    private static void Money(decimal value, bool positive)
    {
        if (value < 0 || (positive && value == 0) || value > 9999999999999999.99m || FiscalValidation.Money(value) != value)
            throw new ConflictException("Informe um valor monetário válido com até duas casas decimais.");
    }
    private static void ValidateRequest(Guid id, DateTimeOffset at)
    {
        if (id == Guid.Empty || at == default || at > DateTimeOffset.UtcNow.AddMinutes(5))
            throw new ConflictException("Informe identificador da requisição e data válida, sem data futura.");
    }
    private static PaymentResponse Map(Payment p, IReadOnlyDictionary<Guid, FiscalState> docs)
    {
        var settled = p.Movements.Where(x => x.Kind == "Settlement").Sum(x => x.Amount);
        var reversed = p.Movements.Where(x => x.Kind == "Reversal").Sum(x => x.Amount);
        return new(p.Id, p.Amount, p.Method, p.ReceivedAt, settled, reversed, p.Amount - settled,
            p.Allocations.Select(x => new PaymentAllocationResponse(x.DocumentId, x.Amount,
                !docs.TryGetValue(x.DocumentId, out var state) || state != FiscalState.Authorized || reversed > 0)).ToList(),
            p.Movements.OrderBy(x => x.CreatedAt).Select(x => new PaymentMovementResponse(x.Id, x.Kind, x.Amount,
                x.Fees, x.SegregatedTax, x.OccurredAt, x.SettlementId, x.Reason)).ToList());
    }
}
