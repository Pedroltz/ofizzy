using FluentValidation;

namespace SportPneus.Api.Modules.Vehicles;

public sealed record VehicleRequest(Guid CustomerId, string Plate, string? Brand, string Model, short? Year, string? Color, int? Mileage, string? Chassis, string? Notes);
public sealed record VehicleResponse(Guid Id, Guid CustomerId, string CustomerName, string Plate, string? Brand, string Model, short? Year, string? Color, int? Mileage, string? Chassis, string? Notes, bool IsActive, DateTimeOffset CreatedAt);

public sealed class VehicleRequestValidator : AbstractValidator<VehicleRequest>
{
    public VehicleRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty(); RuleFor(x => x.Plate).NotEmpty().MinimumLength(7).MaximumLength(8).Matches("^[A-Za-z0-9-]+$");
        RuleFor(x => x.Brand).MaximumLength(80); RuleFor(x => x.Model).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Year).InclusiveBetween((short)1900, (short)(DateTime.UtcNow.Year + 1)).When(x => x.Year.HasValue);
        RuleFor(x => x.Color).MaximumLength(50); RuleFor(x => x.Mileage).GreaterThanOrEqualTo(0).When(x => x.Mileage.HasValue);
        RuleFor(x => x.Chassis).MaximumLength(40); RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
