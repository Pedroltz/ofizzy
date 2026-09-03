using FluentValidation;

namespace SportPneus.Api.Modules.Parts;

public sealed record PartRequest(string Name, string Code, decimal CostPrice, decimal SalePrice);
public sealed record PartResponse(Guid Id, string Name, string Code, decimal CostPrice, decimal SalePrice, bool IsActive);
public sealed class PartRequestValidator : AbstractValidator<PartRequest>
{
    public PartRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160); RuleFor(x => x.Code).NotEmpty().MaximumLength(80);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0).LessThanOrEqualTo(99_999_999_999.99m); RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0).LessThanOrEqualTo(99_999_999_999.99m);
    }
}
