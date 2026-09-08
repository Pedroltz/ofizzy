using FluentValidation;

namespace Ofizzy.Api.Modules.Services;

public sealed record ServiceRequest(string Name, string? Description, decimal DefaultPrice);
public sealed record ServiceResponse(Guid Id, string Name, string? Description, decimal DefaultPrice, bool IsActive);
public sealed class ServiceRequestValidator : AbstractValidator<ServiceRequest>
{
    public ServiceRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160); RuleFor(x => x.Description).MaximumLength(1000); RuleFor(x => x.DefaultPrice).GreaterThanOrEqualTo(0).LessThanOrEqualTo(99_999_999_999.99m);
    }
}
