using Ofizzy.Api.Modules.Tenancy;

namespace Ofizzy.Api.Modules.Payments;

public sealed record PaymentAllocationRequest(Guid DocumentId, decimal Amount);
public sealed record RegisterPaymentRequest(Guid RequestId, decimal Amount, string Method, DateTimeOffset ReceivedAt,
    IReadOnlyList<PaymentAllocationRequest>? Allocations = null);
public sealed record SettlePaymentRequest(Guid RequestId, decimal Amount, decimal Fees, decimal? SegregatedTax, DateTimeOffset SettledAt);
public sealed record ReversePaymentRequest(Guid RequestId, Guid SettlementId, decimal Amount, string Reason, DateTimeOffset ReversedAt);
public sealed record PaymentAllocationResponse(Guid DocumentId, decimal Amount, bool RequiresReview);
public sealed record PaymentMovementResponse(Guid Id, string Kind, decimal Amount, decimal Fees, decimal? SegregatedTax,
    DateTimeOffset OccurredAt, Guid? SettlementId, string? Reason);
public sealed record PaymentResponse(Guid Id, decimal Amount, string Method, DateTimeOffset ReceivedAt, decimal Settled,
    decimal Reversed, decimal Balance, IReadOnlyList<PaymentAllocationResponse> Allocations, IReadOnlyList<PaymentMovementResponse> Movements);
public sealed record PaymentDocumentResponse(Guid Id, string Kind, decimal Total, decimal Available);
public sealed record OrderPaymentsResponse(decimal Total, decimal Registered, decimal Settled, decimal Reversed,
    decimal Balance, IReadOnlyList<PaymentResponse> Payments, IReadOnlyList<PaymentDocumentResponse> Documents);

public sealed class Payment : ITenantScoped
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public Guid WorkOrderId { get; set; }
    public Guid RequestId { get; set; }
    public string RequestHash { get; set; } = "";
    public decimal Amount { get; set; }
    public string Method { get; set; } = "";
    public DateTimeOffset ReceivedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<PaymentAllocation> Allocations { get; set; } = [];
    public List<PaymentMovement> Movements { get; set; } = [];
}

public sealed class PaymentAllocation : ITenantScoped
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public Guid PaymentId { get; set; }
    public Guid DocumentId { get; set; }
    public decimal Amount { get; set; }
}

public sealed class PaymentMovement : ITenantScoped
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public Guid PaymentId { get; set; }
    public Guid RequestId { get; set; }
    public string RequestHash { get; set; } = "";
    public string Kind { get; set; } = "Settlement";
    public decimal Amount { get; set; }
    public decimal Fees { get; set; }
    // Null means segregation not informed; zero means explicitly informed as zero.
    public decimal? SegregatedTax { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public Guid? SettlementId { get; set; }
    public string? Reason { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
