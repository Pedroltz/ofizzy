using FluentValidation;

namespace Ofizzy.Api.Modules.WorkOrders;

public sealed record WorkOrderLineRequest(
    Guid? CatalogId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    string? Code = null);

public sealed record WorkOrderRequest(
    Guid CustomerId,
    Guid? VehicleId,
    int? Mileage,
    string? Complaint,
    string? Diagnosis,
    string? Notes,
    IReadOnlyList<WorkOrderLineRequest> Services,
    IReadOnlyList<WorkOrderLineRequest> Parts);

public sealed record WorkOrderStatusRequest(WorkOrderStatus Status);

public sealed record WorkOrderLineResponse(
    Guid Id,
    Guid? CatalogId,
    string Description,
    string? Code,
    decimal Quantity,
    decimal UnitPrice,
    decimal Total);

public sealed record WorkOrderResponse(
    Guid Id,
    long Number,
    Guid CustomerId,
    Guid? VehicleId,
    string CustomerName,
    string? CustomerDocument,
    string? CustomerPhone,
    string? VehiclePlate,
    string? VehicleDescription,
    int? Mileage,
    string? Complaint,
    string? Diagnosis,
    string? Notes,
    WorkOrderStatus Status,
    decimal ServicesTotal,
    decimal PartsTotal,
    decimal Total,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyList<WorkOrderLineResponse> Services,
    IReadOnlyList<WorkOrderLineResponse> Parts);

public sealed record WorkOrderSummaryResponse(
    Guid Id,
    long Number,
    string CustomerName,
    string? VehiclePlate,
    string? VehicleDescription,
    WorkOrderStatus Status,
    decimal Total,
    DateTimeOffset CreatedAt);

public sealed class WorkOrderRequestValidator : AbstractValidator<WorkOrderRequest>
{
    public WorkOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Mileage).GreaterThanOrEqualTo(0).When(x => x.Mileage.HasValue);
        RuleFor(x => x.Complaint).MaximumLength(3000);
        RuleFor(x => x.Diagnosis).MaximumLength(5000);
        RuleFor(x => x.Notes).MaximumLength(3000);

        RuleForEach(x => x.Services).SetValidator(new WorkOrderLineRequestValidator());
        RuleForEach(x => x.Parts).SetValidator(new WorkOrderLineRequestValidator());

        RuleFor(x => x).Must(x => x.Services.Count + x.Parts.Count <= 100)
            .WithMessage("Uma OS pode ter no máximo 100 itens.");
    }
}

public sealed class WorkOrderLineRequestValidator : AbstractValidator<WorkOrderLineRequest>
{
    public WorkOrderLineRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(9999);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).LessThanOrEqualTo(999999999);
        RuleFor(x => x.Code).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Code));
    }
}
