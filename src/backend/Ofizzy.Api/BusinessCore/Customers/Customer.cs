using Ofizzy.Api.Verticals.Automotive;

namespace Ofizzy.Api.Modules.Customers;

public sealed class Customer : Ofizzy.Api.Modules.Tenancy.ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string? Document { get; set; }
    public string? Phone { get; set; }
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public string? PostalCode { get; set; }
    public string? Street { get; set; }
    public string? Number { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? CityCode { get; set; }
    public string? StateRegistration { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<Vehicle> Vehicles { get; set; } = [];
}
