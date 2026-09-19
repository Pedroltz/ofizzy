using FluentValidation;

namespace Ofizzy.Api.Modules.Customers;

public sealed record CustomerRequest(
    string Name,
    string? Document,
    string? Phone,
    string? WhatsApp,
    string? Email,
    string? Address,
    string? Notes,
    string? PostalCode = null,
    string? Street = null,
    string? Number = null,
    string? District = null,
    string? City = null,
    string? State = null,
    string? CityCode = null,
    string? StateRegistration = null);

public sealed record CustomerResponse(
    Guid Id,
    string Name,
    string? Document,
    string? Phone,
    string? WhatsApp,
    string? Email,
    string? Address,
    string? Notes,
    bool IsActive,
    DateTimeOffset CreatedAt,
    string? PostalCode = null,
    string? Street = null,
    string? Number = null,
    string? District = null,
    string? City = null,
    string? State = null,
    string? CityCode = null,
    string? StateRegistration = null);

public sealed class CustomerRequestValidator : AbstractValidator<CustomerRequest>
{
    public CustomerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(160);

        RuleFor(x => x.Document)
            .Must(value => string.IsNullOrWhiteSpace(value) || new string(value.Where(char.IsDigit).ToArray()).Length is 11 or 14)
            .WithMessage("Informe um CPF com 11 ou CNPJ com 14 dígitos.");

        RuleFor(x => x.Phone)
            .MaximumLength(20);

        RuleFor(x => x.WhatsApp)
            .MaximumLength(20);

        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(254)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Address)
            .MaximumLength(500);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);

        RuleFor(x => x.PostalCode)
            .Matches("^[0-9]{8}$")
            .WithMessage("CEP deve conter 8 dígitos numéricos.")
            .When(x => !string.IsNullOrWhiteSpace(x.PostalCode));

        RuleFor(x => x.Street)
            .MaximumLength(60);

        RuleFor(x => x.Number)
            .MaximumLength(60);

        RuleFor(x => x.District)
            .MaximumLength(60);

        RuleFor(x => x.City)
            .MaximumLength(60);

        RuleFor(x => x.State)
            .Matches("^[A-Z]{2}$")
            .WithMessage("UF deve conter 2 letras maiúsculas.")
            .When(x => !string.IsNullOrWhiteSpace(x.State));

        RuleFor(x => x.CityCode)
            .Matches("^[0-9]{7}$")
            .WithMessage("Código IBGE deve conter 7 dígitos numéricos.")
            .When(x => !string.IsNullOrWhiteSpace(x.CityCode));

        RuleFor(x => x.StateRegistration)
            .MaximumLength(14);
    }
}
