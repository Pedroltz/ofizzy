using FluentValidation;

namespace SportPneus.Api.Modules.Company;

public sealed class UpdateCompanyRequestValidator : AbstractValidator<UpdateCompanyRequest>
{
    public UpdateCompanyRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.LegalName).MaximumLength(160);
        RuleFor(x => x.Cnpj).MaximumLength(18);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.WhatsApp).MaximumLength(20);
        RuleFor(x => x.Email).MaximumLength(254).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.State).MaximumLength(2);
        RuleFor(x => x.PostalCode).MaximumLength(10);
        RuleFor(x => x.WarrantyTerms).MaximumLength(1000);
        RuleFor(x => x.ReceiptNotes).MaximumLength(1000);
    }
}
