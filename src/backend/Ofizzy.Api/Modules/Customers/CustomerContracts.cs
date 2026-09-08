using FluentValidation;

namespace Ofizzy.Api.Modules.Customers;

public sealed record CustomerRequest(string Name, string? Document, string? Phone, string? WhatsApp, string? Email, string? Address, string? Notes);
public sealed record CustomerResponse(Guid Id, string Name, string? Document, string? Phone, string? WhatsApp, string? Email, string? Address, string? Notes, bool IsActive, DateTimeOffset CreatedAt);

public sealed class CustomerRequestValidator : AbstractValidator<CustomerRequest>
{
    public CustomerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Document).Must(value => string.IsNullOrWhiteSpace(value) || new string(value.Where(char.IsDigit).ToArray()).Length is 11 or 14).WithMessage("Informe um CPF com 11 ou CNPJ com 14 dígitos.");
        RuleFor(x => x.Phone).MaximumLength(20); RuleFor(x => x.WhatsApp).MaximumLength(20);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(254).When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Address).MaximumLength(500); RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
