using SportPneus.Api.Modules.Customers;

namespace SportPneus.Api.Modules.Vehicles;

public sealed class Vehicle
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string Plate { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string Model { get; set; } = string.Empty;
    public short? Year { get; set; }
    public string? Color { get; set; }
    public int? Mileage { get; set; }
    public string? Chassis { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
