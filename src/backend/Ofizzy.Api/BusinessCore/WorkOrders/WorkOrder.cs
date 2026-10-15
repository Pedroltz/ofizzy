using Ofizzy.Api.Modules.Customers;
using Ofizzy.Api.Modules.Parts;
using Ofizzy.Api.Modules.Services;
using Ofizzy.Api.Verticals.Automotive;

namespace Ofizzy.Api.Modules.WorkOrders;

public enum WorkOrderStatus { Open, InProgress, Completed, Cancelled }

public sealed class WorkOrder : Ofizzy.Api.Modules.Tenancy.ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public long Number { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerDocument { get; set; }
    public string? CustomerPhone { get; set; }
    public string? VehiclePlate { get; set; }
    public string? VehicleDescription { get; set; }
    public int? Mileage { get; set; }
    public string? Complaint { get; set; }
    public string? Diagnosis { get; set; }
    public string? Notes { get; set; }
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Open;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public List<WorkOrderService> Services { get; set; } = [];
    public List<WorkOrderPart> Parts { get; set; } = [];
}

public sealed class WorkOrderService : Ofizzy.Api.Modules.Tenancy.ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;
    public Guid? ServiceId { get; set; }
    public ServiceItem? Service { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public sealed class WorkOrderPart : Ofizzy.Api.Modules.Tenancy.ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;
    public Guid? PartId { get; set; }
    public Part? Part { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Code { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
